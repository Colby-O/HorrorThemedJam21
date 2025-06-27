using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    public class MirrorCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _renderInterval;
        [SerializeField] private float _renderOffset;

        [SerializeField] private MeshRenderer _screen;
        [SerializeField] private int _materialIndex;
        [SerializeField] private Vector2Int _texSize = new Vector2Int(512, 512);

        [SerializeField, ReadOnly] private bool _isEnabled;
        [SerializeField, ReadOnly] private float _timeSinceLastRender;

        [SerializeField, ReadOnly] private RenderTexture _tex;

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

        private void Update()
        {
            if (!_isEnabled) return;

            _timeSinceLastRender += Time.deltaTime;

            if (_timeSinceLastRender > _renderInterval && _camera != null)
            {
                _camera.Render();
                _timeSinceLastRender = 0;
            }
        }
    }
}
