using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    public class MirrorCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _renderInterval;
        [SerializeField] private float _renderOffset;
        [SerializeField] private bool _enablePerspective = false;

        [SerializeField] private MeshRenderer _screen;
        [SerializeField] private int _materialIndex;
        [SerializeField] private Vector2Int _texSize = new Vector2Int(512, 512);

        [SerializeField, ReadOnly] private bool _isEnabled;
        [SerializeField, ReadOnly] private float _timeSinceLastRender;

        [SerializeField, ReadOnly] private RenderTexture _tex;

        private Camera _playerCamera;

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        public void CreateRenderTexture()
        {
            if (_tex != null) _tex.Release();
            _tex = new RenderTexture(_texSize.x, _texSize.y, 0);
            _camera.targetTexture = _tex;
        }

        private bool IsVisible(Renderer renderer, Camera camera)
        {
            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustum, renderer.bounds);
        }

        private void Render()
        {
            _screen.enabled = false;

            Vector3 mirrorPosition = transform.position;
            Vector3 playerPosition = _playerCamera.transform.position;

            Vector3 offset = -transform.forward * 0.01f;

            _camera.transform.position = mirrorPosition + offset;

            Vector3 lookDirection = (playerPosition - mirrorPosition).normalized;

            _camera.transform.rotation = Quaternion.LookRotation(lookDirection, Vector3.up);

            _camera.Render();

            _screen.enabled = true;
        }

        private void Awake()
        {
            if (_camera == null) _camera = GetComponent<Camera>();
            if (_camera != null)
            {
                _camera.enabled = false;
                if (_camera.targetTexture == null)
                {
                    CreateRenderTexture();
                }
                else
                {
                    _tex = _camera.targetTexture;
                }
            }

            if (_screen != null)
            {
                _screen.materials[_materialIndex].mainTexture = _tex;
            }

            _timeSinceLastRender = Random.Range(0, _renderInterval);
            Enable();
        }

        private void Start()
        {
            _playerCamera = HTJ21GameManager.Player.GetCamera();
        }

        private void LateUpdate()
        {
            if (!_isEnabled) return;

            _timeSinceLastRender += Time.deltaTime;

            if (_timeSinceLastRender > _renderInterval && _camera != null)
            {
                if (_enablePerspective) Render();
                else _camera.Render();
                _timeSinceLastRender = 0;
            }
        }
    }
}
