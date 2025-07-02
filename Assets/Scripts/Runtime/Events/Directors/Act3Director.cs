using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace HTJ21
{
    public class Act3Director : Director
    {
        [SerializeField] private List<GameObject> _items;

        [SerializeField] private PlayerInSpotlight _moonController;

        [Header("Sections")]
        [SerializeField] private GameObject _roadSection;
        [SerializeField] private GameObject _roomSection;
        [SerializeField] private GameObject _showcaseSection;

        [SerializeField] private GameObject _moonBeam;

        [SerializeField] private Transform _void;
        [SerializeField] private float _voidStartHeight = 45.79f;
        [SerializeField] private float _voidShowcaseHeight;

        [Header("Dialogue")]
        [SerializeField] private DialogueSO _startDialogue;

        [Header("Music")]
        [SerializeField] private float _fadeTime = 5f;
        [SerializeField] private AudioSource _mainSource;
        [SerializeField] private float _mainSourceVolume;
        [SerializeField] private AudioSource _chantSource;
        [SerializeField] private Transform _heightTarget;
        [SerializeField] private Transform _heightStart;
        private bool _stopChant = false;

        [Header("References")]
        [SerializeField] List<ShowInRange> _stages;
        [SerializeField] private Door _enterRoomDoor;
        [SerializeField] private Door _exitRoomDoor;
        [SerializeField] private Portal _toEpilogue;
        [SerializeField] private Portal _atEpilogue;
        [SerializeField] private GameObject _terrian;

        [Header("Checkpoints")]
        [SerializeField] private List<Transform> _checkpoints;

        [SerializeField, ReadOnly] private int _currentCheckpoint;

        private void StopAllMusic(bool force)
        {
            if (force)
            {
                _mainSource.Stop();
                _chantSource.Stop();
            }
            else
            {
                float sv = _mainSource.volume;
                float cv = _chantSource.volume;
                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(HTJ21GameManager.Instance, _fadeTime, (float t) => AudioHelper.FadeOut(_mainSource, sv, 0f, t));
                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(HTJ21GameManager.Instance, _fadeTime, (float t) => AudioHelper.FadeOut(_chantSource, cv, 0f, t));
            }
        }

        public void ResetPlayer()
        {
            if (_currentCheckpoint >= _checkpoints.Count || _currentCheckpoint < 0) return;

            if (HTJ21GameManager.Player)
            {
                HTJ21GameManager.Player.Teleport(_checkpoints[_currentCheckpoint].position);
            }
        }

        public void NextCheckpoint()
        {
            _currentCheckpoint++;
        }

        private void OpenPortals()
        {
            _toEpilogue.Enable();
            _atEpilogue.Enable();
        }

        private void ClosePortals()
        {
            _toEpilogue.Disable();
            _atEpilogue.Disable();
        }

        private void OnPortalEnter(Portal p1, Portal p2)
        {
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        private void RestartInteractablesRecursive(GameObject obj)
        {
            if (obj.TryGetComponent(out IInteractable interactable))
            {
                interactable.Restart();
            }

            foreach (Transform child in obj.transform)
            {
                RestartInteractablesRecursive(child.gameObject);
            }
        }

        private void Setup()
        {
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Player);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Car);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Inspector);

            HTJ21GameManager.Car.RestartHitch();
            HTJ21GameManager.Car.Restart();
            HTJ21GameManager.Car.ExitCar(true);
            HTJ21GameManager.Car.DisableMirrors();
            HTJ21GameManager.CinematicCar.Enable();

            HTJ21GameManager.Player.EnablePlayer();
            HTJ21GameManager.Player.StopLookAt();
            HTJ21GameManager.Player.ResetHead();
            HTJ21GameManager.Player.ResetCamera();
            HTJ21GameManager.Player.LockMoving = false;
            HTJ21GameManager.Player.LockMovement = false;
            HTJ21GameManager.Inspector.EndInspect();

            _currentCheckpoint = 0;
            ResetPlayer();

            HTJ21GameManager.PickupManager.DropAll();
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.FlashLight);
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.BathroomKey);
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.BathroomSupplies);

            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.AddListener(OnPortalEnter);
            ClosePortals();

            _enterRoomDoor.Restart();
            _enterRoomDoor.Unlock();

            _exitRoomDoor.Restart();
            _exitRoomDoor.Unlock();

            foreach (GameObject item in _items)
            {
                RestartInteractablesRecursive(item);
            }

            foreach (ShowInRange stage in _stages) stage.Restart();

            if (!GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<GameView>()) GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
            HTJ21GameManager.IsPaused = false;

            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SkipLocation();
            GameManager.GetMonoSystem<IDialogueMonoSystem>().ResetDialogueAll();
            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().ForceStopDialogue();
            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().RestoreDefaults();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().DisableRain();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().DisableThunder();
            GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOff();

            Vector3 voidPos = _void.transform.localPosition;
            voidPos.y = _voidStartHeight;
            _void.transform.localPosition = voidPos;

            StopAllMusic(true);
            float volScale = GameManager.GetMonoSystem<IAudioMonoSystem>().GetOverallVolume() * GameManager.GetMonoSystem<IAudioMonoSystem>().GetMusicVolume();
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(this, _fadeTime, (float t) => AudioHelper.FadeIn(_mainSource, 0f, _mainSourceVolume * volScale, t));

            _chantSource.volume = 0f;
            _chantSource.Play();
            _stopChant = false;

            _moonBeam.SetActive(true);
            _roadSection.SetActive(true);
            _roomSection.SetActive(false);
            _showcaseSection.SetActive(false);

            if (_startDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_startDialogue);
        }

        private void AddEvents()
        {
            GameManager.AddEventListener<Events.VoidNextCheck>(Events.NewVoidNextCheck((from, data) =>
            {
                NextCheckpoint();
            }));

            GameManager.AddEventListener<Events.RoadSectionFinished>(Events.NewRoadSectionFinished((from, data) =>
            {
                _roomSection.SetActive(true);
            }));


            GameManager.AddEventListener<Events.RoomSectionStart>(Events.NewRoomSectionStart((from, data) =>
            {
                _moonBeam.SetActive(false);
                _enterRoomDoor.Close();
                _enterRoomDoor.Lock();
                NextCheckpoint();

                _stopChant = true;
                float cv = _chantSource.volume;
                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(this, _fadeTime, (float t) => AudioHelper.FadeOut(_chantSource, cv, 0f, t));
            }));

            GameManager.AddEventListener<Events.RoomSectionFinished>(Events.NewRoomSectionFinished((from, data) =>
            {
                Vector3 voidPos = _void.transform.localPosition;
                voidPos.y = _voidShowcaseHeight;
                _void.transform.localPosition = voidPos;

                _roadSection.SetActive(false);
                _roomSection.SetActive(false);
                _showcaseSection.SetActive(true);
                _terrian.SetActive(true);
                _exitRoomDoor.Close();
                _exitRoomDoor.Lock();
                OpenPortals();
                NextCheckpoint();

                StopAllMusic(false);
            }));
        }

        public override void OnActEnd()
        {
            ClosePortals();
            StopAllMusic(true);
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.RemoveListener(OnPortalEnter);
        }

        public override void OnActInit()
        {
            ClosePortals();
            AddEvents();
        }

        public override void OnActStart()
        {
            Setup();
        }

        public override void OnActUpdate()
        {
            if (HTJ21GameManager.Player && HTJ21GameManager.Player.transform.position.y <= _void.position.y) 
            { 
                ResetPlayer();
            }

            if (!_stopChant && _chantSource.isPlaying)
            {
                float t = Mathf.Clamp01((HTJ21GameManager.Player.transform.position.y - _heightStart.position.y) / (_heightTarget.position.y - _heightStart.position.y));
                float volScale = GameManager.GetMonoSystem<IAudioMonoSystem>().GetOverallVolume() * GameManager.GetMonoSystem<IAudioMonoSystem>().GetMusicVolume();
                _chantSource.volume = Mathf.Lerp(0f, 1f, t) * volScale;
            }
        }
    }
}
