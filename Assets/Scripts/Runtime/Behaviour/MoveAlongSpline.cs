using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace HTJ21
{
    public class MoveAlongSpline : MonoBehaviour
    {
        public static bool IsTriggering = false;

        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private int _splineIndex = 0;
        [SerializeField] private Vector2 _travelRange = new Vector2Int(0, 1);
        [SerializeField] private float _speed = 0.2f;
        [SerializeField] private bool _useSpeedUpRange;
        [SerializeField] private Vector2 _speedUpRange = new Vector2Int(0, 0);
        [SerializeField] private float _speedUpValue = 0.2f;
        [SerializeField] private bool _canTurn = true;
        [SerializeField] private bool _isLoop = false;
        [SerializeField] private float _turnSpeed = 3f;
        [SerializeField] private float _offsetRight = 0f;
        [SerializeField] private float _offsetUp= 0f;
        [SerializeField] private Vector3 _rotationOffset;

        [Range(0f, 1f)]
        [SerializeField] private float t;

        private bool _movingForward = true;
        private bool _isWaiting = false;
        private float _tolerance = 0.01f;
        private HashSet<int> _activatedKnotIndices = new HashSet<int>();

        public enum Direction { Forward, Backward }

        [System.Serializable]
        public class KnotWait
        {
            [Range(0f, 1f)] public float positionT;
            public float waitTime;
            public Direction direction;
            public Vector3 lookDirection;
        }

        [SerializeField] private List<KnotWait> _knotWaits = new List<KnotWait>();

        public void Stop()
        {
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            _isWaiting = true;
            MoveAlongSpline.IsTriggering = true;
        }

        public void Continue()
        {
            _isWaiting = false;
            MoveAlongSpline.IsTriggering = false;
        }

        private void Update()
        {
            if (_splineContainer == null || _splineContainer.Splines.Count <= _splineIndex || _isWaiting || HTJ21GameManager.IsPaused || MoveAlongSpline.IsTriggering)
                return;

            float length = _splineContainer.Splines[_splineIndex].GetLength();
            float delta = (((_useSpeedUpRange && t >= _speedUpRange.x && t < _speedUpRange.y) ? _speedUpValue : _speed) / length) * Time.deltaTime;
            float previousT = t;

            t += (_movingForward ? delta : -delta);
            t = Mathf.Clamp01(t);

            for (int i = 0; i < _knotWaits.Count; i++)
            {
                var knot = _knotWaits[i];
                bool isInRange = Mathf.Abs(t - knot.positionT) < _tolerance;
                bool cameFromCorrectDir =
                    (_movingForward && knot.direction == Direction.Forward) ||
                    (!_movingForward && knot.direction == Direction.Backward);

                if (isInRange && cameFromCorrectDir && !_activatedKnotIndices.Contains(i))
                {
                    _activatedKnotIndices.Add(i);

                    _isWaiting = true;
                    Quaternion fromRot = transform.rotation;
                    Quaternion toRot = Quaternion.LookRotation(knot.lookDirection.normalized, transform.up);
                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                        this,
                        _canTurn ? _turnSpeed : 0f,
                        (float t) =>
                        {
                            if (_canTurn) transform.rotation = Quaternion.Slerp(fromRot, toRot, t);
                        },
                        () =>
                        {
                            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                this,
                                knot.waitTime,
                                (float t) =>
                                {
                                },
                                () =>
                                {
                                    _splineContainer.Evaluate(_splineIndex, t, out float3 _, out float3 tangent, out float3 upVector);
                                    Vector3 resumeDir = (_movingForward ? 1f : -1f) * ((Vector3)tangent).normalized;
                                    Quaternion resumeRot = Quaternion.LookRotation(resumeDir, upVector);
                                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                        this,
                                        _canTurn ? _turnSpeed : 0f,
                                        (float t) =>
                                        {
                                            if (_canTurn) transform.rotation = Quaternion.Slerp(toRot, resumeRot, t);
                                        },
                                        () =>
                                        {
                                            _isWaiting = false;
                                        }
                                    );
                                }
                            );
                        }
                    );
                    return;
                }
            }

            if (t >= _travelRange.y || t <= _travelRange.x)
            {
                if (_isLoop)
                {
                    _movingForward = true;
                    t = 0f;
                    return;
                }

                _movingForward = t <= _travelRange.x;
                t = _movingForward ? _travelRange.x : _travelRange.y;

                _activatedKnotIndices.Clear();

                if (!_canTurn) return;

                _isWaiting = true;

                _splineContainer.Evaluate(_splineIndex, t, out float3 _, out float3 tangent, out float3 upVector);
                Vector3 oldDir = (_movingForward ? -1f : 1f) * ((Vector3)tangent).normalized;
                Quaternion fromRot = Quaternion.LookRotation(oldDir, upVector);

                Vector3 newDir = (_movingForward ? 1f : -1f) * ((Vector3)tangent).normalized;
                Quaternion toRot = Quaternion.LookRotation(newDir, upVector);

                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                    this,
                    _turnSpeed,
                    (float t) =>
                    {
                        transform.rotation = Quaternion.Slerp(fromRot, toRot, t);
                    },
                    () =>
                    {
                        GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                            this,
                            0.1f,
                            (float t) =>
                            {
                            },
                            () =>
                            {
                                _isWaiting = false;
                            }
                        );
                    }
                );

                return;
            }

            UpdateTransformAlongSpline();
        }

        private void UpdateTransformAlongSpline()
        {
            _splineContainer.Evaluate(_splineIndex, t, out float3 position, out float3 tangent, out float3 upVector);
            float3 right = Vector3.Cross(Vector3.Normalize(tangent), upVector);

            Vector3 lookDir = (_movingForward ? 1f : -1f) * ((Vector3)tangent).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, upVector);

            transform.position = position + _offsetRight * right + _offsetUp * upVector;
            transform.rotation = targetRotation * Quaternion.Euler(_rotationOffset);
        }

        private void OnEnable()
        {
            Continue();
        }

        private void OnDisable()
        {
            Stop();
        }
    }
}
