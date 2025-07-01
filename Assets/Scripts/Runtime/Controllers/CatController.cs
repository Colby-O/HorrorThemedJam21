using PlazmaGames.Attribute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    [System.Serializable]
    public struct CatWaypoint
    {
        public Transform transform;
        public float maxJumpForce;
    }

    public class CatController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Animator _anim;

        [SerializeField] private List<CatWaypoint> _waypoints;
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _obstacleCheckDistance = 1f;
        [SerializeField] private float _surfaceCheckHeight = 2f;
        [SerializeField] private float _jumpThreshold = 1.5f;
        [SerializeField] private float _minJumpDistance = 1.2f;
        [SerializeField] private float _maxJumpDistance = 4f;
        [SerializeField] private float _minWaitTime = 1f;
        [SerializeField] private float _maxWaitTime = 3f;
        [SerializeField] private float _stuckThreshold = 0.1f;
        [SerializeField] private float _stuckTimeLimit = 2f;
        [SerializeField] private float _targetEplision = 0.1f;


        [SerializeField, ReadOnly] private int _currentWaypointIndex = 0;
        [SerializeField, ReadOnly] private bool _isJumping = false;
        [SerializeField, ReadOnly] private bool _isWaiting = false;
        [SerializeField, ReadOnly] private bool _isApproachingJump = false;

        [SerializeField, ReadOnly] private Vector3 _lastPosition;
        [SerializeField, ReadOnly] private float _stuckTimer = 0f;
        [SerializeField, ReadOnly] private float _waitTimer = 0f;
        [SerializeField, ReadOnly] private float _waitTime = 0f;

        private void Awake()
        {
            if (!_rb) _rb = GetComponent<Rigidbody>();
            _currentWaypointIndex = 0;
            StartWaitBeforeMoving();
        }

        private void Update()
        {
            if (_isWaiting)
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer > _waitTime)
                {
                    _waitTimer = 0f;
                    _isWaiting = false;
                }
            }
        }

        private void FixedUpdate()
        {
            _anim.SetBool("IsWalking", !_isWaiting && !_isJumping);
            _anim.SetBool("Jump", !_isWaiting && _isJumping);

            if (_waypoints.Count == 0 || _isJumping || _isWaiting) return;

            Transform target = _waypoints[_currentWaypointIndex].transform;
            Vector3 direction = (target.position - transform.position);
            Vector3 horizontalDirection = new Vector3(direction.x, 0, direction.z).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            float verticalDiff = target.position.y - transform.position.y;

            bool needsToJump = verticalDiff > _jumpThreshold && IsSurfaceAhead(target);
            bool canJumpNow = distanceToTarget <= _maxJumpDistance && distanceToTarget >= _minJumpDistance;

            if (needsToJump)
            {
                if (canJumpNow && IsGrounded())
                {
                    JumpTowards(target.position);
                    return;
                }
                else
                {
                    _isApproachingJump = true;

                    Vector3 moveDir = new Vector3(direction.x, 0, direction.z).normalized;
                    _rb.MovePosition(transform.position + moveDir * _moveSpeed * Time.fixedDeltaTime);

                    if (moveDir != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(moveDir);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
                    }

                    return;
                }
            }

            _isApproachingJump = false;

            Vector3 avoidanceForce = CalculateObstacleAvoidanceForce();

            Vector3 moveDirection = horizontalDirection + avoidanceForce;
            moveDirection.y = 0;
            moveDirection = moveDirection.normalized;

            _rb.MovePosition(transform.position + moveDirection * _moveSpeed * Time.fixedDeltaTime);

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
            }

            if ((new Vector3(direction.x, 0f, direction.z)).magnitude < _targetEplision && !needsToJump)
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
                StartWaitBeforeMoving();
            }

            if ((transform.position - _lastPosition).magnitude < _stuckThreshold)
            {
                _stuckTimer += Time.fixedDeltaTime;
                if (_stuckTimer > _stuckTimeLimit)
                {
                    _currentWaypointIndex = (_currentWaypointIndex + 1) % _waypoints.Count;
                    _stuckTimer = 0f;
                    StartWaitBeforeMoving();
                    return;
                }
            }
            else
            {
                _stuckTimer = 0f;
            }

            _lastPosition = transform.position;
        }

        private Vector3 CalculateObstacleAvoidanceForce()
        {
            Vector3 avoidance = Vector3.zero;
            float avoidDistance = _obstacleCheckDistance;
            float maxAvoidForce = 2f;

            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

            Vector3[] checkDirections = new Vector3[]
            {
                transform.forward,
                Quaternion.Euler(0, 30, 0) * transform.forward,
                Quaternion.Euler(0, -30, 0) * transform.forward,
            };

            foreach (Vector3 dir in checkDirections)
            {
                if (Physics.Raycast(rayOrigin, dir, out RaycastHit hit, avoidDistance))
                {
                    if (!hit.collider.isTrigger)
                    {
                        Vector3 away = (rayOrigin - hit.point).normalized;
                        float strength = (avoidDistance - hit.distance) / avoidDistance;
                        avoidance += away * strength * maxAvoidForce;
                    }
                }
            }

            return avoidance;
        }

        private void StartWaitBeforeMoving()
        {
            _isWaiting = true;
            _waitTimer = 0f;
            _waitTime = Random.Range(_minWaitTime, _maxWaitTime);
        }

        private void JumpTowards(Vector3 target)
        {
            _isJumping = true;

            Vector3 jumpDir = (target - transform.position).normalized;
            jumpDir.y = 1f;

            Vector3 lookDir = new Vector3(jumpDir.x, 0, jumpDir.z);
            if (lookDir != Vector3.zero)
            {
                Quaternion jumpRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, jumpRot, 1f);
            }

            _rb.AddForce(jumpDir.normalized * _waypoints[_currentWaypointIndex].maxJumpForce, ForceMode.VelocityChange);
            Invoke(nameof(EndJump), 1.0f);
        }

        private void EndJump()
        {
            _isJumping = false;
        }

        private bool IsGrounded()
        {
            return Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);
        }

        private bool IsSurfaceAhead(Transform target)
        {
            Vector3 origin = transform.position + Vector3.up * _surfaceCheckHeight;
            Vector3 direction = (target.position - origin).normalized;
            return Physics.Raycast(origin, direction, out RaycastHit hit, 3f);
        }
    }
}
