using UnityEngine;
using UnityEngine.Splines;

namespace HTJ21
{
    [RequireComponent(typeof(Light))]
    public class SplineTargetTracker : MonoBehaviour
    {
        [SerializeField] private int _splineIndex;
        [SerializeField] private float _speed = 0.2f;
        [SerializeField] private float _maxRayDistance = 100f;
        [SerializeField] private LayerMask _mask = ~0;

        [Range(0f, 1f)]
        [SerializeField] private float t;


        private SplineContainer _splineContainer;
        private bool _movingForward = true;

        private void Start()
        {
            _splineContainer = RoadwayCreator.Instance.GetContainer();
        }

        private void Update()
        {
            if (_splineContainer == null || _splineContainer.Spline == null)
                return;

            float length = _splineContainer.Spline.GetLength();
            float delta = (_speed / length) * Time.deltaTime;

            t += (_movingForward ? delta : -delta);

            if (t >= 1f)
            {
                t = 1f;
                _movingForward = false;
            }
            else if (t <= 0f)
            {
                t = 0f;
                _movingForward = true;
            }

            Vector3 targetPosition = _splineContainer.EvaluatePosition(_splineIndex, t);
            Vector3 direction = (targetPosition - transform.position).normalized;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, _maxRayDistance, _mask))
            {
                Vector3 surfacePoint = hit.point;
                transform.rotation = Quaternion.LookRotation((surfacePoint - transform.position).normalized, Vector3.up);
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }
}
