using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    public class MirrorCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _renderInterval;
        [SerializeField] private float _renderOffset;

        [SerializeField, ReadOnly] private bool _isEnabled;
        [SerializeField, ReadOnly] private float _timeSinceLastRender;

        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        private void Awake()
        {
            if (_camera == null) _camera = GetComponent<Camera>();
            if (_camera != null) _camera.enabled = false;
            _timeSinceLastRender = 0;
            Enable();
        }

        private void Update()
        {
            if (!_isEnabled) return;

            _timeSinceLastRender += Time.deltaTime;

            if (_timeSinceLastRender > _renderInterval && _camera != null)
            {
                _camera.Render();
                _timeSinceLastRender = _renderOffset;
            }
        }
    }
}
