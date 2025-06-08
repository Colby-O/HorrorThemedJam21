using UnityEngine;

namespace HTJ21
{
    
    public class ProceduralAudio : MonoBehaviour
    {
    }
    [RequireComponent(typeof(AudioSource))]
    public class EngineSound : MonoBehaviour
    {
        private float _sampleRate;

        [System.Serializable]
        class EngineSoundWave
        {
            public float volume;
            public float overtone;
            public float offset;
            public float dutyScale;
            public float baseDuty;
            public float throttleDutyScale;
        }
        
        [SerializeField] float _freqScale;
        [SerializeField] float _baseDuty;
        [SerializeField] float _throttleDuty;
        [SerializeField] EngineSoundWave[] _waves;

        public bool engineOn = true;
        
        private float _rpm = 0;
        private float _throttle;
        private float[] _phases;

        public void SetRpm(float rpm) => _rpm = rpm;
        public void SetThrottle(float throttle) => _throttle = throttle;

        void Start()
        {
            _sampleRate = AudioSettings.outputSampleRate; // ✅ this runs on main thread
            _phases = new float[_waves.Length];
            for (int i = 0; i < _phases.Length; i++) _phases[i] = 0;
        }

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!engineOn) return;
            
            for (int i = 0; i < data.Length; i++)
            {
                float baseFreq = _rpm * _freqScale;
                float sub = 0;
                float totalVolume = 0;
                for (int j = 0; j < _waves.Length; j++) {
                    EngineSoundWave wave = _waves[j];
                    _phases[j] += baseFreq * wave.overtone / _sampleRate; 
                    if (_phases[j] >= 100.0) _phases[j] -= 100.0f;
                    totalVolume += wave.volume;
                    sub += wave.volume * Mathf.Pow(
                        Mathf.Sin(_phases[j] + wave.offset),
                        Mathf.Floor(
                            wave.baseDuty + _baseDuty * wave.dutyScale +
                            (_throttle > 0.1 ? 1 : 0) * _throttleDuty * wave.throttleDutyScale
                        )
                    );
                }
                for (int j = 0; j < channels; j++) data[i] = sub / totalVolume;
            }
        }
    }
}
