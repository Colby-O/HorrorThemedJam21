using PlazmaGames.Animation;
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
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private float _speed = 0.2f;
        [SerializeField] private float _turnSpeed = 3f;

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
        }

        public void Continue()
        {
            _isWaiting = false;
        }

        private void Update()
        {
            if (_splineContainer == null || _splineContainer.Spline == null || _isWaiting || HTJ21GameManager.IsPaused)
                return;

            float length = _splineContainer.Spline.GetLength();
            float delta = (_speed / length) * Time.deltaTime;
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
                        _turnSpeed,
                        (float t) =>
                        {
                            transform.rotation = Quaternion.Slerp(fromRot, toRot, t);
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
                                    _splineContainer.Evaluate(t, out float3 _, out float3 tangent, out float3 upVector);
                                    Vector3 resumeDir = (_movingForward ? 1f : -1f) * ((Vector3)tangent).normalized;
                                    Quaternion resumeRot = Quaternion.LookRotation(resumeDir, upVector);
                                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                        this,
                                        _turnSpeed,
                                        (float t) =>
                                        {
                                            transform.rotation = Quaternion.Slerp(toRot, resumeRot, t);
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

            if (t >= 1f || t <= 0f)
            {
                _movingForward = t <= 0f;
                t = _movingForward ? 0f : 1f;

                _activatedKnotIndices.Clear();

                _isWaiting = true;

                _splineContainer.Evaluate(t, out float3 _, out float3 tangent, out float3 upVector);
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
            _splineContainer.Evaluate(t, out float3 position, out float3 tangent, out float3 upVector);

            Vector3 lookDir = (_movingForward ? 1f : -1f) * ((Vector3)tangent).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(lookDir, upVector);

            transform.position = position;
            transform.rotation = targetRotation;
        }
    }
}
