using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEditor;
using UnityEngine;

namespace HTJ21
{
    public class HTJ21GameManager : GameManager
    {
        [SerializeField] GameObject _monoSystemHolder;

        [Header("MonoSystems")]
        [SerializeField] private UIMonoSystem _uiSystem;
        [SerializeField] private AnimationMonoSystem _animSystem;
        [SerializeField] private AudioMonoSystem _audioSystem;
        [SerializeField] private WeatherMonoSystem _weatherSystem;
        [SerializeField] private GPSMonoSystem _gpsSystem;
        public static GameObject Player { get; set; }

        private void AttachMonoSystems()
        {
            AddMonoSystem<UIMonoSystem, IUIMonoSystem>(_uiSystem);
            AddMonoSystem<AnimationMonoSystem, IAnimationMonoSystem>(_animSystem);
            AddMonoSystem<AudioMonoSystem, IAudioMonoSystem>(_audioSystem);
            AddMonoSystem<WeatherMonoSystem, IWeatherMonoSystem>(_weatherSystem);
            AddMonoSystem<GPSMonoSystem, IGPSMonoSystem>(_gpsSystem);
        }

        public override string GetApplicationName()
        {
            return nameof(HTJ21GameManager);
        }

        public override string GetApplicationVersion()
        {
            return "v0.0.1";
        }

        protected override void OnInitalized()
        {
            AttachMonoSystems();

            _monoSystemHolder.SetActive(true);
        }

        private void Start()
        {
            Player = GameObject.FindWithTag("Player");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
