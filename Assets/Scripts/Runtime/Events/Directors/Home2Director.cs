using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.Core.Debugging;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class Home2Director : Director
    {
        [SerializeField] private List<GameObject> _items;

        [SerializeField] private Transform _actStartLoc;
        [SerializeField] private Door _bathroomDoor;
        [SerializeField] private Shower _showerController;
        [SerializeField] private Portal _toVoid;
        [SerializeField] private Portal _atVoid;
        [SerializeField] private Door _exitDoor;
        [SerializeField] private SafePad _keypad;

        [Header("Moon")]
        [SerializeField] private MeshRenderer _moon;
        [SerializeField] private Vector3 _moonSize;

        [Header("Dialogue")]
        [SerializeField] private DialogueSO _startDialogue;

        [Header("References")]
        [SerializeField] private GameObject _act3Reference;

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

        private void OpenPortals()
        {
            _toVoid.Enable();
            _atVoid.Enable();
        }

        private void ClosePortals()
        {
            _toVoid.Disable();
            _atVoid.Disable();
        }

        private void OnPortalEnter(Portal p1, Portal p2)
        {
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
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
            HTJ21GameManager.Player.Teleport(_actStartLoc.position);
            HTJ21GameManager.Player.LockMoving = false;
            HTJ21GameManager.Player.LockMovement = false;
            HTJ21GameManager.Inspector.EndInspect();

            HTJ21GameManager.PickupManager.DropAll();
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.FlashLight);
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.BathroomKey);
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.BathroomSupplies);

            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.AddListener(OnPortalEnter);
            ClosePortals();

            _moon.gameObject.SetActive(true);
            _moon.material.SetColor("_BaseColor", HTJ21GameManager.Preferences.MoonRedColor);
            _moon.transform.localScale = _moonSize;

            _exitDoor.Restart();
            _exitDoor.OnOpen.AddListener(OpenPortals);

            _keypad.Restart();

            _bathroomDoor.Restart();
            _bathroomDoor.Lock();

            foreach (GameObject item in _items)
            {
                RestartInteractablesRecursive(item);
            }

            _showerController.StartShower(false);
            _showerController.Disable();

            if (!GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<GameView>()) GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
            HTJ21GameManager.IsPaused = false;

            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SkipLocation();
            GameManager.GetMonoSystem<IDialogueMonoSystem>().ResetDialogueAll();
            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().ForceStopDialogue();
            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().RestoreDefaults();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().EnableRain();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().EnableThunder();
            GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOff();

            if (_startDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_startDialogue);
        }

        private void AddEvents()
        {
            _keypad.OnSolved.AddListener(() =>
            {
                _act3Reference.gameObject.SetActive(true);
            });
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

        public override void OnActEnd()
        {
            ClosePortals();
            _moon.gameObject.SetActive(false);
            _showerController.Restart();
            _exitDoor.OnOpen.RemoveListener(OpenPortals);
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.RemoveListener(OnPortalEnter);
        }
    }
}
