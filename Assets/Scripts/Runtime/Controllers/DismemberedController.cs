using UnityEngine;

namespace HTJ21
{
    public class DismemberedController : MonoBehaviour
    {
        [SerializeField] private Transform _anchor;
        [SerializeField] private LineRenderer _lr;

        private void UpdateRope()
        {
            _lr.SetPosition(0, _anchor.position);
            _lr.SetPosition(1, transform.position);
        }

        private void Awake()
        {
            _lr.positionCount = 2;
        }

        private void Update()
        {
            UpdateRope();
        }
    }
}
