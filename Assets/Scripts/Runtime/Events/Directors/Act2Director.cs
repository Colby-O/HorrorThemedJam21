using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class Act2Director : Director
    {
        [SerializeField] private List<GameObject> _items;

        [SerializeField] private Transform _actStartLoc;

        [SerializeField] private Door _bathroomDoor;
        [SerializeField] private Door _safeDoor;
        [SerializeField] private SafePad _safe;
        [SerializeField] private ItemPickup _bathroomKey;

        [Header("Shower")]
        [SerializeField] private Shower _showerController;

        [Header("Music")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private float _fadeTime;

        [Header("References")]
        [SerializeField] private GameObject _tvObjects;
        [SerializeField] private MeshRenderer _tvScreen;
        [SerializeField] private TVCamera _tvCamera;

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

        private void OnShowerFinished()
        {
            _showerController.RestoreToDefaults();
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        private void StopAllMusic(bool force)
        {
            if (force)
            {
                _musicSource.Stop();
            }
            else
            {
                float sv = _musicSource.volume;
                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(HTJ21GameManager.Instance, _fadeTime, (float t) => AudioHelper.FadeOut(_musicSource, sv, 0f, t));
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
            HTJ21GameManager.Player.Teleport(_actStartLoc.position);
            HTJ21GameManager.Player.LockMoving = false;
            HTJ21GameManager.Player.LockMovement = false;

            HTJ21GameManager.Inspector.EndInspect();

            HTJ21GameManager.PickupManager.DropAll();
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.FlashLight);
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.BathroomSupplies);

            StopAllMusic(true);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(HTJ21GameManager.Instance, _fadeTime, (float t) => AudioHelper.FadeIn(_musicSource, 0f, 1f, t));

            _showerController.Restart();
            _safe.Restart();
            _bathroomKey.Restart();
            _safeDoor.Restart();
            _bathroomDoor.Restart();
            _safeDoor.Lock();
            _bathroomDoor.Lock();

            foreach (GameObject item in _items)
            {
                RestartInteractablesRecursive(item);
            }

            _tvObjects.SetActive(true);
            _tvCamera.SetScreen(_tvScreen);

            _showerController.OnShowerFinish.AddListener(OnShowerFinished);

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
        }

        private void AddEvents()
        {

        }

        public override void OnActEnd()
        {
            StopAllMusic(false);
            _showerController.Restart();
            _tvObjects.SetActive(false);
            _tvCamera.SetScreen(null);
            _showerController.OnShowerFinish.RemoveListener(OnShowerFinished);
        }

        public override void OnActInit()
        {
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
