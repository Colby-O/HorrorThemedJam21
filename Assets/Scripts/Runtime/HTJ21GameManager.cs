using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.Core.Events;
using PlazmaGames.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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
        [SerializeField] private InputMonoSystem _inputSystem;
        [SerializeField] private DialogueMonoSystem _dialogueSystem;
        [SerializeField] private ScreenEffectMonoSystem _screenEffectSystem;

        [Header("Settings")]
        [SerializeField] private GamePreferences preferences;

        public static GamePreferences Preferences => (Instance as HTJ21GameManager).preferences;
        public static PlayerController Player { get; set; }
        public static PickupManager PickupManager { get; set; }
        public static CarController Car { get; set; }
        public static CinematicCarController CinematicCar { get; set; }

        public static bool IsPaused { get; set; }

        public static GameObject CurrentControllable => Player ? (((Player.IsInCar()) ? (Car ? Car.gameObject : null) : Player.gameObject)) : null;

        public static Camera GetActiveCamera()
        {
            if (GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentView<MainMenuView>()) return CinematicCar.GetCamera();
            if (Player && Car && Player.IsInCar()) return Car.GetCamera();
            if (Player) return Player.GetCamera();
            return null;
        }

        private void AttachMonoSystems()
        {
            AddMonoSystem<UIMonoSystem, IUIMonoSystem>(_uiSystem);
            AddMonoSystem<AnimationMonoSystem, IAnimationMonoSystem>(_animSystem);
            AddMonoSystem<AudioMonoSystem, IAudioMonoSystem>(_audioSystem);
            AddMonoSystem<WeatherMonoSystem, IWeatherMonoSystem>(_weatherSystem);
            AddMonoSystem<GPSMonoSystem, IGPSMonoSystem>(_gpsSystem);
            AddMonoSystem<InputMonoSystem, IInputMonoSystem>(_inputSystem);
            AddMonoSystem<DialogueMonoSystem, IDialogueMonoSystem>(_dialogueSystem);
            AddMonoSystem<ScreenEffectMonoSystem, IScreenEffectMonoSystem>(_screenEffectSystem);
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

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            Player = FindAnyObjectByType<PlayerController>();
            Car = FindAnyObjectByType<CarController>();
            if (Car) CinematicCar = Car.GetComponent<CinematicCarController>();
            if (Player) PickupManager = Player.GetComponent<PickupManager>();
        }

        private void OnSceneUnload(Scene scene)
        {
            RemoveAllEventListeners();
        }

        private void Awake()
        {
            SceneManager.LoadScene("Act1");
        }

        private void Start()
        {
            HTJ21GameManager.IsPaused = true;
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            SceneManager.sceneUnloaded -= OnSceneUnload;
        }

        protected override void Update()
        {
            // TODO: Remove me testing code.
            base.Update();
            //if (Keyboard.current.escapeKey.wasPressedThisFrame) SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
