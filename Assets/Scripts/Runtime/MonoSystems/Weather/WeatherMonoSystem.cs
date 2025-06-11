using PlazmaGames.Attribute;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HTJ21
{
    public class WeatherMonoSystem : MonoBehaviour, IWeatherMonoSystem
    {
        [SerializeField] private float _height = 40f;

        [Header("Lighting Settings")]
        [SerializeField, ColorUsage(true, true)] private Color _lightingColor;
        [SerializeField] private Vector2 _lightingInterval;
        [SerializeField] private Vector2 _thunderDelay;
        [SerializeField] private Vector2 _skyDelay;
        [SerializeField] private List<AudioClip> _thunderClips;

        [SerializeField, ReadOnly] private bool _isLightingOn = true;

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

        public void SpawnLightingAt(Vector3 pos)
        {
            _lightingHitter.Emit(1);
            _lightingTimer = 0;
            _thunderTimer = 0;
            _skyTimer = 0;
            _thunderQueued = true;
            _skyQueued = true;
            _timeToNextLighting = Random.Range(_lightingInterval.x, _lightingInterval.y);
            _skylight.gameObject.SetActive(true);
            HTJ21GameManager.GetActiveCamera().backgroundColor = _lightingColor;
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            _weatherGameObject = GameObject.Instantiate(Resources.Load("Prefabs/Weather")) as GameObject;
            _rain = _weatherGameObject.transform.GetChild(0).GetComponent<ParticleSystem>();
            _lighting = _weatherGameObject.transform.GetChild(1).GetComponent<ParticleSystem>();
            _lightingHitter = _weatherGameObject.transform.GetChild(2).GetComponent<ParticleSystem>();
            _skylight = _weatherGameObject.transform.GetChild(3).GetComponent<Light>();

            _skylight.gameObject.SetActive(false);

            _timeToNextLighting = Random.Range(_lightingInterval.x, _lightingInterval.y);
            _timeToNextThunder = Random.Range(_thunderDelay.x, _thunderDelay.y);
            _timeToNextSky = Random.Range(_skyDelay.x, _skyDelay.y);
            _lightingTimer = 0;
            _thunderTimer = 0;
            _skyTimer = 0;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }

        private void Update()
        {
            if (HTJ21GameManager.CurrentControllable != null && _weatherGameObject != null)
            {
                Vector3 playerPos = HTJ21GameManager.CurrentControllable.transform.position;
                playerPos.y += _height;
                _weatherGameObject.transform.position = playerPos;
            }

            if (_skyQueued) _skyTimer += Time.deltaTime;
            if (_skyQueued && _skyTimer > _timeToNextSky)
            {
                _skylight.gameObject.SetActive(false);
                HTJ21GameManager.GetActiveCamera().backgroundColor = Color.black;
                _timeToNextSky = Random.Range(_skyDelay.x, _skyDelay.y);
                _skyQueued = false;
            }

            if (_isLightingOn)
            {
                _lightingTimer += Time.deltaTime;

                if (_thunderQueued) _thunderTimer += Time.deltaTime;

                if (_thunderTimer > _timeToNextThunder && _thunderQueued)
                {
                    //TODO Thunder Audio HERE
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
                    HTJ21GameManager.GetActiveCamera().backgroundColor = _lightingColor;
                }
            }
        }
    }
}
