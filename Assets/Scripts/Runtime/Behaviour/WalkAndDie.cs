using UnityEngine;

namespace HTJ21
{
    public class WalkAndDie : MonoBehaviour
    {
        [SerializeField] private float _speed = 0.3f;
        [SerializeField] private float _walkDistance = 8;

        private Rigidbody _rig;

        private Vector3 _startPosition;
        private bool _walking = false;

        public void Walk()
        {
            _startPosition = transform.position;
            _walking = true;
        }

        private void Awake()
        {
            _rig = GetComponent<Rigidbody>();
        }
        
        private void FixedUpdate()
        {
            if (!_walking) return;
            _rig.linearVelocity = transform.forward * _speed;
            if (Vector3.Distance(_startPosition, transform.position) > _walkDistance)
            {
                Destroy(gameObject);
            }
        }
    }
}
