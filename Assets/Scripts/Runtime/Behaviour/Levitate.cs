using UnityEngine;

namespace HTJ21
{
    public class Levitate : MonoBehaviour
    {
        [SerializeField] private float _height = 0.5f;
        [SerializeField] private float _speed = 0.2f;
        [SerializeField] private bool _levitating = true;
        
        private float _startHeight;
        private float _phase = 0;
        [SerializeField] private bool _stopAtBase = false;
        [SerializeField] private bool _stopAtPeak = false;

        public void StartLevitate() => _levitating = true;
        public void StopLevitate() => _levitating = false;

        public void StopLevitatingAtNextBase()
        {
            _stopAtBase = true;
        }
        
        public void StopLevitatingAtNextPeak()
        {
            _stopAtPeak = true;
        }

        public void StartLevitateFromPosition()
        {
            _levitating = true;   
            _startHeight = transform.position.y;
        }

        private void Start()
        {
            _startHeight = transform.position.y;
        }

        private void Update()
        {
            if (!_levitating) return;
            int baseBefore = Mathf.FloorToInt(_phase);
            int peakBefore = Mathf.RoundToInt(_phase);
            
            _phase += Time.deltaTime * _speed;
            Vector3 pos = transform.position;
            pos.y = _startHeight + _height * (0.5f - 0.5f * Mathf.Cos(_phase * 2.0f * Mathf.PI));

            int baseAfter = Mathf.FloorToInt(_phase);
            int peakAfter = Mathf.RoundToInt(_phase);
            if (_stopAtBase && baseBefore != baseAfter)
            {
                pos.y = _startHeight;
                _levitating = false;
                _stopAtBase = false;
            }
            if (_stopAtPeak && peakBefore != peakAfter)
            {
                pos.y = _startHeight + _height;
                _levitating = false;
                _stopAtPeak = false;
            }
            
            transform.position = pos;
        }
    }
}
