using UnityEngine;

namespace HTJ21
{
    public class TorchFlicker : MonoBehaviour
    {
        [SerializeField] private float _flickerSpeed = 0.7f;
        [SerializeField] private float _flickerMagnitude = 0.3f;

        private Color _colorFrom;
        private Color _colorTo;
        private float _startTime = 0;

        private Light _light;

        private void Next()
        {
            _startTime = Time.time;
            _colorFrom = _colorTo;
            float h, s, v;
            Color.RGBToHSV(_colorFrom, out h, out s, out v);
            s = Mathf.Lerp(1.0f - _flickerMagnitude, 1, UnityEngine.Random.value);
            v = Mathf.Lerp(1.0f - _flickerMagnitude, 1, UnityEngine.Random.value);
            _colorTo = Color.HSVToRGB(h, s, v);
        }

        private void Start()
        {
            _light = GetComponentInChildren<Light>();
            _colorTo = _light.color;
            _colorFrom = _light.color;
        }

        private void Update()
        {
            float t = (Time.time - _startTime) / _flickerSpeed;
            if (t >= 1)
            {
                t = 0;
                Next();
            }

            _light.color = Color.Lerp(_colorFrom, _colorTo, t);
        }
    }
}
