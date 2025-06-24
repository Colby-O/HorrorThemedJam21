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

        public void Fall()
        {
            _fell = true;
            _startRotation = transform.localEulerAngles;
            if (_fallRotation.x == 0) _fallRotation.x = _startRotation.x;
            if (_fallRotation.y == 0) _fallRotation.y = _startRotation.y;
            if (_fallRotation.z == 0) _fallRotation.z = _startRotation.z;
            _startTime = Time.time;
        }
        
        void Update()
        {
            if (_fell && Time.time <= _startTime + _fallTime)
            {
                float t = Mathf.Pow((Time.time - _startTime) / _fallTime, _fallExponent);
                transform.localEulerAngles = Vector3.Lerp(_startRotation, _fallRotation, t);
            }
        }
    }
}
