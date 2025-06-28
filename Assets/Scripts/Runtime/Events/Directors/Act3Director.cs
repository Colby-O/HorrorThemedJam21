using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

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

        [Header("References")]
        [SerializeField] List<ShowInRange> _stages;
        [SerializeField] private Door _enterRoomDoor;
        [SerializeField] private Door _exitRoomDoor;
        [SerializeField] private Portal _toEpilogue;
        [SerializeField] private Portal _atEpilogue;

        [Header("Checkpoints")]
        [SerializeField] private List<Transform> _checkpoints;

        [SerializeField, ReadOnly] private int _currentCheckpoint;

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

            _moonBeam.SetActive(true);
            _roadSection.SetActive(true);
            _roomSection.SetActive(false);
            _showcaseSection.SetActive(false);
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
            }));

            GameManager.AddEventListener<Events.RoomSectionFinished>(Events.NewRoomSectionFinished((from, data) =>
            {
                _roadSection.SetActive(false);
                _roomSection.SetActive(false);
                _showcaseSection.SetActive(true);
                _exitRoomDoor.Close();
                _exitRoomDoor.Lock();
                OpenPortals();
                NextCheckpoint();
            }));
        }

        public override void OnActEnd()
        {
            ClosePortals();
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

        }
    }
}
