using UnityEngine;

namespace HTJ21
{
    public class TVCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _renderRate = 0f;

        private float _timer = 0f;

        private MeshRenderer _screen;

        public void SetScreen(MeshRenderer screen) => _screen = screen;

        private bool IsVisible(Renderer renderer, Camera camera)
        {
            Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(frustum, renderer.bounds);
        }

        private void Render()
        {
            if (!_screen || !IsVisible(_screen, HTJ21GameManager.Player.GetCamera())) return;
            _camera.Render();
        }

        private void Awake()
        {
            if (!_camera) _camera = GetComponent<Camera>();

            _camera.enabled = false;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer > _renderRate)
            {
                Render();
                _timer = 0f;
            }
        }
    }
}
