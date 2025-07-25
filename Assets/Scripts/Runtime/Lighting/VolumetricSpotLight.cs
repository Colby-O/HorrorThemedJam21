using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    [ExecuteAlways, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(Light))]
    public class VolumetricSpotLight : MonoBehaviour
    {
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private MeshRenderer _meshRenderer;
        [SerializeField] private Light _light;
        [SerializeField, Range(0f, 1f)] private float _opacity = 0.3f;
        [SerializeField, Range(4, 128)] private int _segments = 24;
        [SerializeField, Range(0f, 1f)] private float _startFadeDistance = 0.8f; 
        [SerializeField, Range(0f, 100f)] private float fadeSharpness = 2f;
        [SerializeField] private bool _setUpdateLimit = false;
        [SerializeField] private float _updateRate = 0.1f;
        [SerializeField, ReadOnly] private float _timer;
        private Mesh _mesh;

        private Vector3 _lastPosition;
        private Quaternion _lastRotation;
        private float _lastRange;
        private float _lastAngle;
        private float _lastOpacity;

        private bool LightChanged()
        {
            return _light.transform.position != _lastPosition ||
                   _light.transform.rotation != _lastRotation ||
                   _light.range != _lastRange ||
                   _light.spotAngle != _lastAngle ||
                   _opacity != _lastOpacity;
        }

        private void CacheLightState()
        {
            _lastPosition = _light.transform.position;
            _lastRotation = _light.transform.rotation;
            _lastRange = _light.range;
            _lastAngle = _light.spotAngle;
            _lastOpacity = _opacity;
        }

        private void Setup()
        {
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
            if (!_light) _light = GetComponent<Light>();

            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
            _meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        }

        private Mesh GenerateMesh()
        {
            if (_mesh == null) _mesh = new Mesh();
            else _mesh.Clear();
            _mesh.name = "VolumetricSpotlightMesh";

            float angleRad = _light.spotAngle * 0.5f * Mathf.Deg2Rad;
            float radius = Mathf.Tan(angleRad) * _light.range;

            Vector3[] vertices = new Vector3[_segments + 2];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[_segments * 3];

            Vector3 rayOrigin = transform.position;

            float[] hitDistances = new float[_segments + 1];
            Vector3[] hitPoints = new Vector3[_segments + 1];
            Vector3[] hitNormals = new Vector3[_segments + 1];
            bool[] hitFlags = new bool[_segments + 1];

            for (int i = 0; i <= _segments; i++)
            {
                float frac = (float)i / _segments;
                float theta = frac * Mathf.PI * 2;

                Vector3 dirLocal = new Vector3(Mathf.Sin(theta) * radius, Mathf.Cos(theta) * radius, _light.range);
                Vector3 dirWorld = transform.TransformPoint(dirLocal);
                Vector3 rayDir = (dirWorld - rayOrigin).normalized;

                if (Physics.Raycast(rayOrigin, rayDir, out RaycastHit hit, _light.range))
                {
                    hitDistances[i] = hit.distance;
                    hitPoints[i] = hit.point;
                    hitNormals[i] = hit.normal;
                    hitFlags[i] = true;
                }
                else
                {
                    hitDistances[i] = _light.range;
                    hitPoints[i] = Vector3.zero;
                    hitNormals[i] = Vector3.zero;
                    hitFlags[i] = false;
                }
            }

            for (int i = 0; i <= _segments; i++)
            {
                float frac = (float)i / _segments;
                float theta = frac * Mathf.PI * 2;
                Vector3 coneDirLocal = new Vector3(Mathf.Sin(theta) * radius, Mathf.Cos(theta) * radius, _light.range);
                Vector3 coneDirWorld = transform.TransformPoint(coneDirLocal);
                Vector3 coneDir = (coneDirWorld - rayOrigin).normalized;

                Vector3 pointWorld;

                if (hitFlags[i])
                {
                    pointWorld = hitPoints[i];
                }
                else
                {
                    bool found = false;
                    Vector3 fallbackPoint = Vector3.zero;
                    Vector3 fallbackNormal = Vector3.up;

                    for (int offset = 1; offset <= _segments / 2; offset++)
                    {
                        int left = i - offset;
                        int right = i + offset;

                        if (left >= 0 && hitFlags[left])
                        {
                            fallbackPoint = hitPoints[left];
                            fallbackNormal = hitNormals[left];
                            found = true;
                            break;
                        }

                        if (right <= _segments && hitFlags[right])
                        {
                            fallbackPoint = hitPoints[right];
                            fallbackNormal = hitNormals[right];
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        Plane fallbackPlane = new Plane(fallbackNormal, fallbackPoint);
                        if (fallbackPlane.Raycast(new Ray(rayOrigin, coneDir), out float projDist))
                            pointWorld = rayOrigin + coneDir * projDist;
                        else
                            pointWorld = rayOrigin + coneDir * _light.range;
                    }
                    else
                    {
                        pointWorld = rayOrigin + coneDir * _light.range;
                    }
                }

                Vector3 localPoint = transform.InverseTransformPoint(pointWorld);
                vertices[i + 1] = localPoint;

                float dst = localPoint.z; 
                float fadeStart = _startFadeDistance * _light.range;
                float t = Mathf.Clamp01((dst - fadeStart) / (_light.range - fadeStart));
                float fade = Mathf.Pow(1 - t, fadeSharpness);
                colors[i + 1] = new Color(_light.color.r, _light.color.g, _light.color.b, fade * _opacity);
            }

            vertices[0] = Vector3.zero;
            colors[0] = new Color(_light.color.r, _light.color.g, _light.color.b, _opacity);

            for (int i = 0; i < _segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = (i + 2 > _segments) ? 1 : i + 2;
            }

            _mesh.SetVertices(vertices);
            _mesh.SetColors(colors);
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            return _mesh;
        }

        private void OnEnable()
        {
            Setup();
            _timer = Random.Range(0.0f, _updateRate);
        }

        private void Update()
        {
            if (_light.type != LightType.Spot) return;

            if (_setUpdateLimit)
            {
                _timer += Time.deltaTime;
                if (_timer > _updateRate) _timer = 0;
                else return;
            }

            if (LightChanged())
            {
                GenerateMesh();
                _meshFilter.sharedMesh = _mesh;
                CacheLightState();
            }

            if (_meshRenderer.sharedMaterial)
            {
                _meshRenderer.sharedMaterial.color = new Color(_light.color.r, _light.color.g, _light.color.b, _opacity);
            }
        }
    }
}
