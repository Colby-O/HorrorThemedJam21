using UnityEngine;

namespace HTJ21
{
    public class TreeFall : MonoBehaviour
    {
        [SerializeField] private Vector3 _fallRotation;
        [SerializeField] private float _fallTime = 3;
        [SerializeField] private float _fallExponent = 2;

        private bool _fell = false;
        private Quaternion _startRotation;
        private float _startTime;

        public void Fall()
        {
            _fell = true;
            _startRotation = transform.rotation;
            _startTime = Time.time;
        }
        
        void Update()
        {
            if (_fell && Time.time <= _startTime + _fallTime)
            {
                float t = Mathf.Pow((Time.time - _startTime) / _fallTime, _fallExponent);
                transform.rotation = Quaternion.Lerp(_startRotation, Quaternion.Euler(_fallRotation), t);
            }
        }
    }
}
