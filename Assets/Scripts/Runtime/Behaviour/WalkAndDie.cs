using UnityEngine;

namespace HTJ21
{
    public class WalkAndDie : MonoBehaviour
    {
        [SerializeField] private float _speed = 0.3f;
        [SerializeField] private float _walkDistance = 8;

        [SerializeField] private Rigidbody _rig;
        [SerializeField] private Animator _animator;

        private Vector3 _startPosition;
        private Quaternion _startRot;
        private bool _walking = false;

        public void Walk()
        {
            _startPosition = transform.position;
            _startRot = transform.rotation;
            _walking = true;
        }

        public void Restart()
        {
            gameObject.SetActive(true);
            transform.position = _startPosition;
            transform.rotation = _startRot;
            _rig.linearVelocity = Vector3.zero;
            _walking = false;
        }

        private void Awake()
        {
            if (!_rig) _rig = GetComponent<Rigidbody>();
            if (!_animator) _animator = GetComponent<Animator>();

            if (_animator) _animator.SetBool("IsWalking", true);

            _startPosition = transform.position;
            _startRot = transform.rotation;
        }
        
        private void FixedUpdate()
        {
            if (!_walking) return;
            _rig.linearVelocity = transform.forward * _speed;
            if (Vector3.Distance(_startPosition, transform.position) > _walkDistance)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
