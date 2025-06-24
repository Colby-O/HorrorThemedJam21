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
        [Range(0f, 1f)] public float _opacity = 0.3f;
        [Range(4, 128)] public int _segments = 24;

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
            float radius = Mathf.Tan(angle * Mathf.Deg2Rad) * _light.range;

            Vector3[] vertices = new Vector3[_segments + 2];
            Color[] colors = new Color[vertices.Length];
            int[] triangles = new int[_segments * 3];

            vertices[0] = Vector3.zero; 
            colors[0] = new Color(_light.color.r, _light.color.g, _light.color.b, _opacity); 

            for (int i = 0; i <= _segments; i++)
            {
                float frac = (float)i / _segments;
                float theta = frac * Mathf.PI * 2;

                float x = Mathf.Sin(theta) * radius;
                float y = Mathf.Cos(theta) * radius;
                vertices[i + 1] = new Vector3(x, y, _light.range);
                colors[i + 1] = new Color(_light.color.r, _light.color.g, _light.color.b, 0f);
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

            if (_meshRenderer.sharedMaterial)
            {
                _meshRenderer.sharedMaterial.color = new Color(_light.color.r, _light.color.g, _light.color.b, _opacity);
            }
        }
    }
}
