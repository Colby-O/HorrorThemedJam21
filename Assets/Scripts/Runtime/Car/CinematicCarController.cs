using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System;
using System.Collections.Generic;
using PlazmaGames.Core.Utils;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Time = UnityEngine.Time;

namespace HTJ21
{
    [Serializable]
    struct CinematicTransform
    {
        public Vector3 Position;
        public Vector3 Rotation;

        public CinematicTransform(Vector3 position, Vector3 rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
        }
    }

    public class CinematicCarController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SplineContainer _path;
        [SerializeField] Camera _camera;
        [SerializeField] Camera _cameraMain;
        [SerializeField] Transform _cameraTarget;
        [SerializeField] Rigidbody _rb;

        [Header("Settings")]
        [SerializeField] private float _speed;
        [SerializeField] private float _yOffset = 1f;
        [SerializeField] private float _xOffset = 0;
        [SerializeField] private float _exitTime = 2f;
        [SerializeField] private List<int> _stopSigns;
        [SerializeField] private float _stopTime = 1f;
        [SerializeField] private float _stopSpeed = 1000f;


        [SerializeField, ReadOnly] private bool _enabled = true;
        [SerializeField, ReadOnly] private bool _isStopped = false;
        [SerializeField, ReadOnly] private List<int> _currentStops;
        [SerializeField, ReadOnly] private float _currentT;
        [SerializeField, ReadOnly] private int _currentKnot;
        [SerializeField, ReadOnly] private CinematicTransform _currentTransform;
        [SerializeField, ReadOnly] private CinematicTransform _endTransform;

        public void Enable()
        {
            SnapCarToClosestKnot();
            _camera.gameObject.SetActive(true);
            _cameraMain.gameObject.SetActive(false);
            _enabled = true;
        }

        public void Disable()
        {
            _camera.gameObject.SetActive(false);
            _cameraMain.gameObject.SetActive(true);
           _enabled = false;
        }

        private void StopCinematicStep(float t, CinematicTransform start, CinematicTransform end)
        {
            Quaternion startRot = Quaternion.Euler(start.Rotation);
            Quaternion endRot = Quaternion.Euler(end.Rotation);

            _camera.transform.localPosition = Vector3.Lerp(start.Position, end.Position, t);
            _camera.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
        }

        public void StopCinematic()
        {
            _cameraMain.gameObject.SetActive(true);
            HTJ21GameManager.Car.SetDisableState(true);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this, 
                _exitTime, 
                (float t) => StopCinematicStep(t, _currentTransform, _endTransform), 
                () => { _enabled = false; _rb.isKinematic = false; _rb.linearVelocity = _stopSpeed * transform.forward; HTJ21GameManager.Car.SetDisableState(false); Disable(); }
            );
        }

        private int GetNextKnot(int currentIndex)
        {
            Spline spline = _path.Spline;
            return (currentIndex == spline.Count - 1) ? 0 : currentIndex + 1;
        }

        private void LookTowardsNext(float t)
        {
            Spline spline = _path.Spline;
            float tNext = t + 0.01f;
            if (tNext > 1f) tNext = 0f;
            Vector3 moveDir = Vector3.Normalize(_path.EvaluatePosition(0, tNext) - _path.EvaluatePosition(0, t));
            transform.rotation = Quaternion.LookRotation(moveDir);
        }

        private void SnapCarToClosestKnot()
        {
            float minDst = float.MaxValue;
            Vector3 minPos = Vector3.zero;
            int minIndex = -1;

            Spline spline = _path.Spline;
            for (int i = 0; i < spline.Count; i++)
            {
                BezierKnot knot = spline[i];
                Vector3 pos = _path.transform.TransformPoint(knot.Position);
                float dst = Vector3.Distance(transform.position, pos);

                if (dst < minDst)
                {
                    minDst = dst;
                    minPos = pos;
                    minIndex = i;
                }
            }

            int nextIndex = (minIndex == spline.Count - 1) ? 0 : minIndex + 1;
            Vector3 moveDir = (_path.transform.TransformPoint(spline[nextIndex].Position) - minPos).normalized;
            _currentT = RoadwayHelper.GetKnotTInSpline(_path, 0, minIndex);
            _currentKnot = minIndex;

            LookTowardsNext(_currentT);
            _path.Evaluate(0, _currentT, out float3 position, out float3 tangent, out float3 upVector);
            transform.position = (Vector3)position + Vector3.Normalize(upVector) * _yOffset;
            _currentStops = new List<int>(_stopSigns);
            Stop();
        }

        private void Stop()
        {
            if (_currentStops.Contains(_currentKnot))
            {
                _currentStops.Remove(_currentKnot);
                _isStopped = true;
                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                    this,
                    _stopTime,
                    (float t) => { },
                    () => { _isStopped = false; }
                );
            }
        }

        private void Move()
        {
            if (_isStopped) return;
            
            _currentT += _speed * Time.deltaTime;

            if (_currentT > 1f)
            {
                _currentStops = new List<int>(_stopSigns);
                _currentT = 0f;
                _currentKnot = 0;
                Stop();
            }
            else
            {
                if (_currentT >= RoadwayHelper.GetKnotTInSpline(_path, 0, _currentKnot))
                {
                    _currentKnot = GetNextKnot(_currentKnot);
                    Stop();
                }
            }

            LookTowardsNext(_currentT);
            _path.Evaluate(0, _currentT, out float3 position, out float3 tangent, out float3 upVector);
            Vector3 right = Vector3.Cross(Vector3.Normalize(tangent), Vector3.Normalize(Vector3.up));
            transform.position = (Vector3)position + Vector3.Normalize(upVector) * _yOffset + right * _xOffset;
        }

        private void Start()
        {
            if (!_rb) _rb = GetComponent<Rigidbody>();
            _currentTransform = new CinematicTransform(_camera.transform.localPosition, _camera.transform.localRotation.eulerAngles);
            _endTransform = new CinematicTransform(_cameraTarget.localPosition, _cameraTarget.localRotation.eulerAngles);
            Enable();
        }

        private void Update()
        {
            if (_enabled) Move();
        }
    }
}
