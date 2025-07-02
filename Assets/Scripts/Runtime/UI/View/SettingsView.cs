using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.Core.Utils;
using PlazmaGames.DataPersistence;
using PlazmaGames.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HTJ21
{
    [System.Serializable]
    public struct ResolutionType
    {
        public int width;
        public int height;

        public string GetName()
        {
            return $"{width}x{height}";
        }
    }

    public class SettingsView : View, IDataPersistence
    {
        [SerializeField] private EventButton _back;

        [SerializeField] private EventButton _openGameplayMenu;
        [SerializeField] private EventButton _openAudioMenu;
        [SerializeField] private EventButton _openVideoMenu;

        [SerializeField] private GameObject _gameplay;
        [SerializeField] private GameObject _video;
        [SerializeField] private GameObject _audio;

        [SerializeField] private GameObject _gameplaySelect;
        [SerializeField] private GameObject _videoSelect;
        [SerializeField] private GameObject _audioSelect;

        [Header("Gameplay Controls")]
        [SerializeField] private PlayerSettings _playerSettings;
        [SerializeField] private Slider _sensitivity;
        [SerializeField] private Toggle _invertX;
        [SerializeField] private Toggle _invertY;
        [SerializeField] private Slider _dialogueSpeed;
        [SerializeField] private TMP_Dropdown _language;
        [SerializeField] private EventButton _car;
        [SerializeField] private TMP_Text _carText;
        [SerializeField] private GameObject _carIcon;

        [Header("Audio Controls")]
        [SerializeField] private Slider _overall;
        [SerializeField] private Slider _sfx;
        [SerializeField] private Slider _music;

        [Header("Video Controls")]
        [SerializeField] private List<ResolutionType> _resolutionTypes;
        [SerializeField] private TMP_Dropdown _resolution;
        [SerializeField] private Toggle _fullscreen;
        [SerializeField] private Toggle _vsync;
        [SerializeField] private EventButton _applyVideo;

        [SerializeField, ReadOnly] private bool _hasInitParams = false;
        [SerializeField, ReadOnly] private int _currentResType = 0;

        private float GetSensitivityAdjustedValue(float input, float exp = 2f)
        {
            return Mathf.Pow(input, exp);
        }

        private void OnOverallChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetOverallVolume(val);
        }

        private void OnMusicChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetMusicVolume(val);
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetAmbientVolume(val);
        }

        private void OnSfXChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetSfXVolume(val);
        }

        private void OnSensitivityChanged(float val)
        {
            float sens = Mathf.Lerp(0.01f, 1f, GetSensitivityAdjustedValue(val));
            _playerSettings.Sensitivity = new Vector2(sens, sens);
        }

        private void OnXInvertChanged(bool val)
        {
            _playerSettings.InvertLookY = val;
        }

        private void OnYInvertChanged(bool val)
        {
            _playerSettings.InvertLookX = val;
        }

        private void OnDialogueSpeedChanged(float val)
        {
            HTJ21GameManager.Preferences.DialogueSpeedMul = Mathf.Lerp(2f, 0f, val);
        }

        private void OnFullscreenChanged(bool val)
        {
            if (Screen.fullScreen != val) Screen.fullScreen = val;
        }

        private void OnVSyncChanged(bool val)
        {
            QualitySettings.vSyncCount = val ? 1 : 0;
        }

        private void OpenGameplay()
        {
            _gameplay.SetActive(true);
            _video.SetActive(false);
            _audio.SetActive(false);

            _gameplaySelect.SetActive(true);
            _videoSelect.SetActive(false);
            _audioSelect.SetActive(false);
        }

        private void OpenAudio()
        {
            _gameplay.SetActive(false);
            _video.SetActive(false);
            _audio.SetActive(true);

            _gameplaySelect.SetActive(false);
            _videoSelect.SetActive(false);
            _audioSelect.SetActive(true);
        }

        private void OpenVideo()
        {
            _gameplay.SetActive(false);
            _video.SetActive(true);
            _audio.SetActive(false);

            _gameplaySelect.SetActive(false);
            _videoSelect.SetActive(true);
            _audioSelect.SetActive(false);
        }

        private void ApplyVideoSettings()
        {
            _currentResType = _resolution.value;
            Screen.SetResolution(_resolutionTypes[_resolution.value].width, _resolutionTypes[_resolution.value].height, _fullscreen.isOn);
        }

        private void Back()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
        }

        private void CarFix()
        {
            if (HTJ21GameManager.CurrentControllable && GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentDirector() && (GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentAct() == Act.Act1 || GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentAct() == Act.Epilogue))
            {
                Vector3 newPos = GameManager.GetMonoSystem<IGPSMonoSystem>().GetClosestNodePositionToPoint(RoadwayHelper.GetRoadways(RoadwayCreator.Instance.GetContainer()), HTJ21GameManager.CurrentControllable.transform.position);
                newPos.y += 5f;
                HTJ21GameManager.CurrentControllable.transform.position = newPos;
                HTJ21GameManager.CurrentControllable.transform.rotation = Quaternion.identity;
                if (TryGetComponent(out Rigidbody rb))
                {
                    rb.linearVelocity = Vector3.zero;
                }
            }
        }

        public override void Init()
        {
            _back.onPointerDown.AddListener(Back);

            _car.onPointerDown.AddListener(CarFix);
            _car.Icon = _carIcon;
            _carIcon.SetActive(false);
            _car.Text = _carText;

            _openGameplayMenu.onPointerDown.AddListener(OpenGameplay);
            _openAudioMenu.onPointerDown.AddListener(OpenAudio);
            _openVideoMenu.onPointerDown.AddListener(OpenVideo);

            _overall.onValueChanged.AddListener(OnOverallChanged);
            _music.onValueChanged.AddListener(OnMusicChanged);
            _sfx.onValueChanged.AddListener(OnSfXChanged);

            _sensitivity.onValueChanged.AddListener(OnSensitivityChanged);
            _invertX.onValueChanged.AddListener(OnXInvertChanged);
            _invertY.onValueChanged.AddListener(OnYInvertChanged);
            _dialogueSpeed.onValueChanged.AddListener(OnDialogueSpeedChanged);
            DropdownUtilities.SetDropdownOptions<Language>(ref _language);

            _fullscreen.onValueChanged.AddListener(OnFullscreenChanged);
            _vsync.onValueChanged.AddListener(OnVSyncChanged);
            List<string> resTypes = new List<string>();
            foreach (ResolutionType res in _resolutionTypes) resTypes.Add(res.GetName());
            DropdownUtilities.SetDropdownOptions(ref _resolution, resTypes);
            _applyVideo.onPointerDown.AddListener(ApplyVideoSettings);

            _overall.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetOverallVolume();
            _music.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetMusicVolume();
            _sfx.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetSfXVolume();

            if (!GameManager.GetMonoSystem<IDataPersistenceMonoSystem>().IsGameLoaded())
            {
                LoadData(new HTJ21GameData());
            }
        }

        public override void Show()
        {
            base.Show();
            OpenGameplay();

            _car.IsDisabled = GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentDirector() == null || (GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentAct() != Act.Act1 && GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentAct() != Act.Epilogue);

            GameManager.GetMonoSystem<IDataPersistenceMonoSystem>().LoadGame();
        }

        public override void Hide()
        {
            base.Hide();
            GameManager.GetMonoSystem<IDataPersistenceMonoSystem>().SaveGame();
        }

        public bool SaveData<TData>(ref TData data) where TData : GameData
        {
            HTJ21GameData gameData = data as HTJ21GameData;
            Debug.Log("Saving Settings");
            if (gameData != null)
            {
                gameData.yInvert = _invertY.isOn;
                gameData.xInvert = _invertX.isOn;
                gameData.sensitivity = _sensitivity.value;
                gameData.dialogueSpeed = _dialogueSpeed.value;
                gameData.language = (Language)_language.value;

                gameData.isFullscreen = _fullscreen.isOn;
                gameData.isVsyncOn = _vsync.isOn;
                gameData.resolutionType = _currentResType;

                return true;
            }

            return false;
        }

        public bool LoadData<TData>(TData data) where TData : GameData
        {
            HTJ21GameData gameData = data as HTJ21GameData;

            Debug.Log("Loading Settings");

            if (gameData != null)
            {
                if (_hasInitParams) return true;
                _hasInitParams = true;
                _invertY.isOn = gameData.yInvert;
                _invertX.isOn = gameData.xInvert;
                _sensitivity.value = gameData.sensitivity;
                _dialogueSpeed.value = gameData.dialogueSpeed;
                _language.value = (int)gameData.language;

                _fullscreen.isOn = gameData.isFullscreen;
                _vsync.isOn = gameData.isVsyncOn;
                _resolution.value = gameData.resolutionType;

                _overall.value = gameData.overallVolume;
                _music.value = gameData.musicVolume;
                _sfx.value = gameData.soundVolume;

                OnXInvertChanged(_invertX.isOn);
                OnYInvertChanged(_invertY.isOn);
                OnSensitivityChanged(_sensitivity.value);
                OnDialogueSpeedChanged(_dialogueSpeed.value);
                
                OnFullscreenChanged(_fullscreen.isOn);
                OnVSyncChanged(_vsync.isOn);
                ApplyVideoSettings();

                OnOverallChanged(_overall.value);
                OnMusicChanged(_music.value);
                OnSfXChanged(_sfx.value);

                return true;
            }

            return false;
        }
    }
}
