using UnityEngine;

namespace HTJ21
{
    public class TreeFall : MonoBehaviour
    {
        [SerializeField] private Vector3 _fallRotation;
        [SerializeField] private float _fallTime = 3;
        [SerializeField] private float _fallExponent = 2;

        private bool _fell = false;
        private Vector3 _startRotation;
        private float _startTime;

        private Vector3 _startPos;
        private Quaternion _startRot;

        public void Fall()
        {
            _fell = true;
            _startRotation = transform.localEulerAngles;
            if (_fallRotation.x == 0) _fallRotation.x = _startRotation.x;
            if (_fallRotation.y == 0) _fallRotation.y = _startRotation.y;
            if (_fallRotation.z == 0) _fallRotation.z = _startRotation.z;
            _startTime = Time.time;
        }

        public void Restart()
        {
            transform.position = _startPos;
            transform.rotation = _startRot;
            _fell = false;
        }

        private void Awake()
        {
            _startPos = transform.position;
            _startRot = transform.rotation;
        }

        private void Update()
        {
            if (_fell && Time.time <= _startTime + _fallTime)
            {
                float t = Mathf.Pow((Time.time - _startTime) / _fallTime, _fallExponent);
                transform.localEulerAngles = Vector3.Lerp(_startRotation, _fallRotation, t);
            }
        }
    }
}
