using PlazmaGames.Attribute;
using PlazmaGames.Core.Debugging;
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
        private Mesh _mesh;

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
            Mesh mesh = new Mesh();
            mesh.name = "VolumetricSpotlightMesh";

            float angle = _light.spotAngle * 0.5f;
            float maxRadius = Mathf.Tan(angle * Mathf.Deg2Rad) * _light.range;

            Vector3[] vertices = new Vector3[_segments + 2];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[_segments * 3];

            float radius = Mathf.Tan(angle * Mathf.Deg2Rad) * _light.range;

            for (int i = 0; i <= _segments; i++)
            {
                float frac = (float)i / _segments;
                float theta = frac * Mathf.PI * 2;

                float x = Mathf.Sin(theta) * radius;
                float y = Mathf.Cos(theta) * radius;
                Vector3 frustumEdgeLocal = new Vector3(x, y, _light.range);

                Vector3 worldTarget = transform.TransformPoint(frustumEdgeLocal);
                Vector3 worldOrigin = transform.position;
                Vector3 worldDir = (worldTarget - worldOrigin).normalized;

                float distance = _light.range;

                if (Physics.Raycast(worldOrigin, worldDir, out RaycastHit hit, _light.range))
                {
                    distance = hit.distance;
                }

                Vector3 hitPointWorld = worldOrigin + worldDir * distance;
                Vector3 localPoint = transform.InverseTransformPoint(hitPointWorld);
                vertices[i + 1] = localPoint;

                float dst = localPoint.magnitude;
                float fadeStart = _startFadeDistance * _light.range;
                float t = Mathf.Clamp01((dst - fadeStart) / (_light.range - fadeStart));
                float fade = Mathf.Pow(1 - t, fadeSharpness);
                colors[i + 1] = new Color(_light.color.r, _light.color.g, _light.color.b, fade * _opacity);
            }

            for (int i = 0; i < _segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2 > _segments ? 1 : i + 2;
            }

            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private void OnEnable()
        {
            Setup();
        }

        private void Update()
        {
            if (_light.type != LightType.Spot) return;

            _mesh = GenerateMesh();
            _meshFilter.sharedMesh = _mesh;
        }
    }
}
