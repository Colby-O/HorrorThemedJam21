using PlazmaGames.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private Portal _linkedPortal;
        [SerializeField] private MeshRenderer _screen;

        [SerializeField] private Camera _portalCamera;
        [SerializeField] private Camera _playerCamera;
        [SerializeField, ReadOnly] private RenderTexture _viewTex;

        [SerializeField] private bool _enableTranslation = true;
        [SerializeField] private bool _enableRotation = true;
        [SerializeField] private bool _enableNearClipCorrection = true;

        [SerializeField] private float nearClipOffset = 0.05f;
        [SerializeField] private float nearClipLimit = 0.2f;

        [SerializeField, ReadOnly] private List<PortalObject> _nearbyObjects;
        [SerializeField, ReadOnly] private bool _isEnabled = true;

        public Renderer GetScreen()
        {
            return _screen;
        }

        public void OnObjectEnter(PortalObject obj)
        {
            if (!_nearbyObjects.Contains(obj))
            {
                obj.PreviousOffsetFromPortal = obj.transform.position - transform.position;
                _nearbyObjects.Add(obj);
                obj.OnPortalEnter?.Invoke(this, _linkedPortal);
            }
        }

        public void OnObjectExit(PortalObject obj)
        {
            _nearbyObjects.Remove(obj);
        }
        float SetThickness(Vector3 viewPoint)
        {
            float halfHeight = _playerCamera.nearClipPlane * Mathf.Tan(_playerCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * _playerCamera.aspect;
            float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, _playerCamera.nearClipPlane).magnitude;
            float screenThickness = dstToNearClipPlaneCorner;

            bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
            _screen.transform.localScale = new Vector3(_screen.transform.localScale.x, _screen.transform.localScale.y, screenThickness);
            _screen.transform.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
            return screenThickness;
        }

        private void CreateRenderTexture()
        {
            if (_enableRotation == false)
            {
                _linkedPortal?._screen?.material.EnableKeyword("_USEOBJECTMODE");
            }
            else
            {
                _linkedPortal?._screen?.material.DisableKeyword("_USEOBJECTMODE");
            }

            if (_viewTex == null || _viewTex.width != Screen.width || _viewTex.height != Screen.height)
            {
                if (_viewTex != null) _viewTex.Release();
                _viewTex = new RenderTexture(Screen.width, Screen.height, 0);
                _portalCamera.targetTexture = _viewTex;
                _linkedPortal._screen.material.SetTexture("_MainTex", _viewTex);
            }
        }

        private bool IsVisible(Renderer renderer, Camera camera)
        {
            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustum, renderer.bounds);
        }

        private void SetNearClipPlane()
        {
            Transform clipPlane = transform;
            int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - _portalCamera.transform.position));

            Vector3 camSpacePos = _portalCamera.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
            Vector3 camSpaceNormal = _portalCamera.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
            float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

            if (Mathf.Abs(camSpaceDst) > nearClipLimit)
            {
                Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

                _portalCamera.projectionMatrix = _playerCamera.CalculateObliqueMatrix(clipPlaneCameraSpace);
            }
            else
            {
                _portalCamera.projectionMatrix = _playerCamera.projectionMatrix;
            }
        }

        private void Render()
        {
            if (!_isEnabled || !IsVisible(_linkedPortal.GetScreen(), _playerCamera)) return;


            _screen.enabled = false;

            CreateRenderTexture();

            Matrix4x4 matrix = transform.localToWorldMatrix * _linkedPortal.transform.worldToLocalMatrix * _playerCamera.transform.localToWorldMatrix;
            _portalCamera.transform.SetPositionAndRotation((_enableTranslation ? matrix.GetColumn(3) : _portalCamera.transform.position), (_enableRotation ? matrix.rotation : _portalCamera.transform.rotation));

            if (_enableNearClipCorrection) SetNearClipPlane();
            SetThickness(_playerCamera.transform.position);
            _portalCamera.Render();

            _screen.enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PortalObject obj))
            {
                OnObjectEnter(obj);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PortalObject obj))
            {
                OnObjectExit(obj);
            }
        }

        private void Awake()
        {
            //_playerCamera = HTJ21GameManager.Player.GetCamera();
            _nearbyObjects = new List<PortalObject>();
            if (!_portalCamera) _portalCamera.GetComponentInChildren<Camera>();
            _portalCamera.enabled = false;
        }

        private void Update()
        {
            Render();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _nearbyObjects.Count; i++) 
            { 
                PortalObject obj = _nearbyObjects[i];

                Vector3 offset = obj.transform.position - transform.position;
                int side = System.Math.Sign(Vector3.Dot(offset, transform.forward));
                int previousSide = System.Math.Sign(Vector3.Dot(_nearbyObjects[i].PreviousOffsetFromPortal, transform.forward));
                if (previousSide != side) 
                {
                    Matrix4x4 matrix = _linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * _nearbyObjects[i].transform.localToWorldMatrix;
                    _nearbyObjects[i].Teleport(matrix.GetColumn(3), matrix.rotation);
                    _linkedPortal.OnObjectEnter(_nearbyObjects[i]);
                    OnObjectExit(_nearbyObjects[i]);
                }
                else _nearbyObjects[i].PreviousOffsetFromPortal = offset;
            }
        }
    }
}
