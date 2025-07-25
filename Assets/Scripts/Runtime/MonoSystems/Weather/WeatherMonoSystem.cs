using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HTJ21
{
    public class WeatherMonoSystem : MonoBehaviour, IWeatherMonoSystem
    {
        [SerializeField] private float _height = 40f;

        [Header("Rain Settings")]
        [SerializeField] private AudioClip _indoorRainClip;
        [SerializeField] private AudioClip _outdoorRainClip;
        private AudioSource _rainAS;

        [Header("Lighting Settings")]
        [SerializeField, ColorUsage(true, true)] private Color _lightingColor;
        [SerializeField] private Vector2 _lightingInterval;
        [SerializeField] private Vector2 _thunderDelay;
        [SerializeField] private Vector2 _skyDelay;
        [SerializeField] private List<AudioClip> _thunderClips;
        [SerializeField] private AudioClip _loudThunderClip;

        [SerializeField, ReadOnly] private bool _isLightingOn = true;
        [SerializeField, ReadOnly] private bool _isRainOn = true;


        private ParticleSystem _rain;
        private ParticleSystem _lighting;
        private ParticleSystem _lightingHitter;

        private Light _skylight;

        private GameObject _weatherGameObject;

        private bool _thunderQueued = false;
        private bool _skyQueued = false;

        private float _timeToNextLighting;
        private float _timeToNextThunder;
        private float _timeToNextSky;

        private float _lightingTimer = 0;
        private float _thunderTimer = 0;
        private float _skyTimer = 0;

        private bool _isIndoors = false;

        public ParticleSystem GetThunderHitter()
        {
            return _lightingHitter.GetComponent<ParticleSystem>();
        }

        public void DisableThunder() => _isLightingOn = false;
        public void EnableThunder() => _isLightingOn = true;

        public void EnableRain()
        {
            _isRainOn = true;
            _rain.Play();
            if (!_rainAS) return;
            _rainAS.clip = _isIndoors ? _indoorRainClip : _outdoorRainClip;
            _rainAS.Play();
        }
        public void DisableRain()
        {
            _isRainOn = false;
            _rain.Stop(); 
            if (!_rainAS) return;
            _rainAS.clip = null;
            _rainAS.Stop();
        }

        public void SetRainState(bool isIndoors)
        {
            if (!_isRainOn || isIndoors == _isIndoors || _rain.isStopped) return;
            _isIndoors = isIndoors;
            if (!_rainAS) return;
            _rainAS.Stop();
            _rainAS.clip = _isIndoors ? _indoorRainClip : _outdoorRainClip;
            _rainAS.Play();
        }

        public void SpawnLightingAt(Vector3 pos)
        {
            _lightingHitter.transform.parent = null;
            pos.y = HTJ21GameManager.CurrentControllable.transform.position.y + _height;
            _lightingHitter.transform.position = pos;
            _lightingHitter.Emit(1);
            _lightingTimer = 0;
            _thunderTimer = 0;
            _skyTimer = 0;
            _thunderQueued = true;
            _skyQueued = true;
            _timeToNextLighting = Random.Range(_lightingInterval.x, _lightingInterval.y);
            _skylight.gameObject.SetActive(true);
            HTJ21GameManager.GetActiveCamera().backgroundColor = _lightingColor;
            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_loudThunderClip, PlazmaGames.Audio.AudioType.Sfx, loop: false);
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {

        }

        private void PlayThunderSound()
        {
            if (_thunderClips == null || _thunderClips.Count == 0) return;
            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_thunderClips[Random.Range(0, _thunderClips.Count)], PlazmaGames.Audio.AudioType.Sfx, loop: false);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }

        private void Awake()
        {
            _weatherGameObject = GameObject.Instantiate(Resources.Load("Prefabs/Weather")) as GameObject;
            _weatherGameObject.name = "====Weather====";
            _rain = _weatherGameObject.transform.GetChild(0).GetComponent<ParticleSystem>();
            _lighting = _weatherGameObject.transform.GetChild(1).GetComponent<ParticleSystem>();
            _lightingHitter = _weatherGameObject.transform.GetChild(2).GetComponent<ParticleSystem>();
            _skylight = _weatherGameObject.transform.GetChild(3).GetComponent<Light>();
            _rainAS = _weatherGameObject.transform.GetChild(4).GetComponent<AudioSource>();

            _skylight.gameObject.SetActive(false);

            _timeToNextLighting = Random.Range(_lightingInterval.x, _lightingInterval.y);
            _timeToNextThunder = Random.Range(_thunderDelay.x, _thunderDelay.y);
            _timeToNextSky = Random.Range(_skyDelay.x, _skyDelay.y);
            _lightingTimer = 0;
            _thunderTimer = 0;
            _skyTimer = 0;

            EnableRain();
            EnableThunder();
            DontDestroyOnLoad(_weatherGameObject);
        }

        private void Update()
        {
            if (!HTJ21GameManager.HasStarted && _weatherGameObject != null)
            {
                Vector3 playerPos = GameManager.GetMonoSystem<IUIMonoSystem>().GetView<MainMenuView>().GetCamera().transform.position;
                playerPos.y += _height;
                _weatherGameObject.transform.position = playerPos;
            }
            else if (HTJ21GameManager.CurrentControllable && _weatherGameObject != null)
            {
                Vector3 playerPos = HTJ21GameManager.CurrentControllable.transform.position;
                playerPos.y += _height;
                _weatherGameObject.transform.position = playerPos;
            }

            if (HTJ21GameManager.Player != null)
            {
                SetRainState(HTJ21GameManager.Player.CheckIfInDoors());
            }

            if (_skyQueued) _skyTimer += Time.deltaTime;
            if (_skyQueued && _skyTimer > _timeToNextSky)
            {
                _skylight.gameObject.SetActive(false);
                if (HTJ21GameManager.GetActiveCamera()) HTJ21GameManager.GetActiveCamera().backgroundColor = Color.black;
                if (HTJ21GameManager.Player && HTJ21GameManager.Player.GetCamera()) HTJ21GameManager.Player.GetCamera().backgroundColor = Color.black;
                if (HTJ21GameManager.Car && HTJ21GameManager.Car.GetCamera()) HTJ21GameManager.Car.GetCamera().backgroundColor = Color.black;
                if (HTJ21GameManager.CinematicCar && HTJ21GameManager.CinematicCar.GetCamera()) HTJ21GameManager.CinematicCar.GetCamera().backgroundColor = Color.black;
                _timeToNextSky = Random.Range(_skyDelay.x, _skyDelay.y);
                _skyQueued = false;
            }

            if (_isLightingOn)
            {
                _lightingTimer += Time.deltaTime;

                if (_thunderQueued) _thunderTimer += Time.deltaTime;

                if (_thunderTimer > _timeToNextThunder && _thunderQueued)
                {
                    PlayThunderSound();
                    _timeToNextThunder = Random.Range(_thunderDelay.x, _thunderDelay.y);
                    _thunderQueued = false;
                }

                if (_lightingTimer > _timeToNextLighting)
                {
                    _lighting.Emit(1);

                    _lightingTimer = 0;
                    _thunderTimer = 0;
                    _skyTimer = 0;
                    _thunderQueued = true;
                    _skyQueued = true;
                    _timeToNextLighting = Random.Range(_lightingInterval.x, _lightingInterval.y);
                    _skylight.gameObject.SetActive(true);
                    if (HTJ21GameManager.GetActiveCamera()) HTJ21GameManager.GetActiveCamera().backgroundColor = _lightingColor;
                }
            }
        }
    }
}
