using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class Home1Director : Director
    {
        [SerializeField] private List<GameObject> _items;

        [Header("Fake Jumpscare")]
        [SerializeField] private EventTrigger _fakeScareTrigger;
        [SerializeField] private float _lightOffDuration;
        [SerializeField] private AudioClip _tensionClip;
        
        [Header("Real Jumpscare")]
        [SerializeField] private EventTrigger _realScareTrigger;
        [SerializeField] private float _turnDuration;
        [SerializeField] private float _numFlickersReal;
        [SerializeField] private AudioClip _buildUp1;
        [SerializeField] private AudioClip _buildUp2;
        [SerializeField] private float _waitDuration;
        [SerializeField] private AudioClip _jumpscareTrack;

        [Header("Light Flickering")]
        [SerializeField] private bool _isFlickering;
        [SerializeField] private float _flickerRate = 1;
        [SerializeField] private float _timer = 0f;
        [SerializeField, ReadOnly] private bool _isOn;
        [SerializeField, ReadOnly] private bool _isClam;

        [Header("Shower")]
        [SerializeField] private Shower _showerController;

        [Header("Refereces")]
        [SerializeField] private EventTrigger _musicChangeTrigger;
        [SerializeField] private GameObject _jumpscare;
        [SerializeField] private Transform _startLoc;

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

        private void StartFlickering()
        {
            HTJ21GameManager.HouseController.TurnOnLights();
            _isFlickering = true;
            _timer = 0f;
            _isOn = true;
        }

        private void StopFlickering()
        {
            HTJ21GameManager.HouseController.TurnOnLights();
            _isFlickering = false;
            _timer = 0f;
            _isOn = true;
        }

        private void FakeScare()
        {
            HTJ21GameManager.HouseController.TurnOffAllLights();
            HTJ21GameManager.HouseController.LockAllDoors();

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _lightOffDuration,
                (float t) =>
                {

                },
                () =>
                {
                    HTJ21GameManager.HouseController.UnlockDoors();
                    StartFlickering();
                    GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                    GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_tensionClip, PlazmaGames.Audio.AudioType.Music, true, false);
                    _isClam = true;
                }
            );
        }

        private void FlickerStep(float t, ref bool isOn, ref float step)
        {
            if (_numFlickersReal > 0 && t > step)
            {
                if (isOn)
                {
                    HTJ21GameManager.HouseController.TurnOffAllLights();
                }
                else
                {
                    HTJ21GameManager.HouseController.TurnOnLights();
                }

                isOn = !isOn;
                step += 1f / (float)_numFlickersReal;
            }
        }

        private void RealScare()
        {
            StopFlickering();
            HTJ21GameManager.HouseController.TurnOffAllLights();
            HTJ21GameManager.HouseController.LockAllDoors();
            HTJ21GameManager.Player.LockMoving = true;

            _showerController.Disable();

            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_buildUp2, PlazmaGames.Audio.AudioType.Music, false, false);

            bool isOn = false;
            float step = 0;

            Quaternion startRot = HTJ21GameManager.Player.transform.rotation;
            Quaternion endRot = Quaternion.LookRotation((_jumpscare.transform.position - HTJ21GameManager.Player.transform.position).normalized, Vector3.up);

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _turnDuration,
                (float t) =>
                {
                    FlickerStep(t, ref isOn, ref step);
                    HTJ21GameManager.Player.transform.rotation = Quaternion.Lerp(startRot, endRot, step);
                },
                () =>
                {
                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                        this,
                        _buildUp2.length + _waitDuration,
                        (float t) =>
                        {
                            FlickerStep(t, ref isOn, ref step);
                        },
                        () =>
                        {
                            HTJ21GameManager.Player.LookAt(_jumpscare.transform);
                            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_jumpscareTrack, PlazmaGames.Audio.AudioType.Music, false, false);
                            _jumpscare.gameObject.SetActive(true);
                            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                this,
                                _jumpscareTrack.length,
                                (float t) =>
                                {
                                    FlickerStep(t, ref isOn, ref step);
                                },
                                () =>
                                {
                                    GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                                    HTJ21GameManager.Player.LockMoving = false;
                                    HTJ21GameManager.Player.StopLookAt();
                                    HTJ21GameManager.HouseController.TurnOnLights();
                                    HTJ21GameManager.HouseController.UnlockDoors();
                                    StopFlickering();
                                    _showerController.Enable();
                                    _jumpscare.gameObject.SetActive(false);
                                    _musicChangeTrigger.gameObject.SetActive(false);
                                }
                            );
                        }
                    );
                }
            );
        }

        private void AddEvents()
        {
            GameManager.AddEventListener<Events.Home1FakeScare>(Events.NewHome1FakeScare((from, data) =>
            {
                FakeScare();
            }));

            GameManager.AddEventListener<Events.Home1RealScare>(Events.NewHome1RealScare((from, data) =>
            {
                RealScare();
            }));
            GameManager.AddEventListener<Events.Home1ToggleMusic>(Events.NewHome1ToggleMusic((from, data) =>
            {
                if (_isClam)
                {
                    GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                    GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_buildUp1, PlazmaGames.Audio.AudioType.Music, false, false);
                    _isClam = false;
                }
                else
                {
                    GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                    GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_tensionClip, PlazmaGames.Audio.AudioType.Music, false, false);
                    _isClam = true;
                }
            }));
        }

        private void OnShowerFinished()
        {
            _showerController.RestoreToDefaults();
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        private void TurnOffTriggers()
        {
            _fakeScareTrigger.gameObject.SetActive(false);
            _realScareTrigger.gameObject.SetActive(false);
            _musicChangeTrigger.gameObject.SetActive(false);
            _jumpscare.SetActive(false);
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
            HTJ21GameManager.Player.Teleport(_startLoc.position);
            HTJ21GameManager.Player.LockMoving = false;
            HTJ21GameManager.Player.LockMovement = false;

            HTJ21GameManager.Inspector.EndInspect();

            HTJ21GameManager.PickupManager.DropAll();
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.FlashLight);

            _showerController.Restart();

            foreach (GameObject item in _items)
            {
                RestartInteractablesRecursive(item);
            }

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

            HTJ21GameManager.HouseController.UnlockDoors();
            HTJ21GameManager.HouseController.TurnOnLights();

            _fakeScareTrigger.gameObject.SetActive(true);
            _realScareTrigger.gameObject.SetActive(true);
            _musicChangeTrigger.gameObject.SetActive(true);
            _isFlickering = false;

            _fakeScareTrigger.Restart();
            _realScareTrigger.Restart();
            _musicChangeTrigger.Restart();
        }

        public override void OnActInit()
        {
            TurnOffTriggers();
            AddEvents();
        }

        public override void OnActStart()
        {
            Setup();
        }

        public override void OnActUpdate()
        {
            if (_isFlickering)
            {
                _timer += Time.deltaTime;

                if (_timer > _flickerRate)
                {
                    if (_isOn)
                    {
                        HTJ21GameManager.HouseController.TurnOffAllLights();
                    }
                    else
                    {
                        HTJ21GameManager.HouseController.TurnOnLights();
                    }

                    _timer = 0;
                    _isOn = !_isOn;
                }
            }
        }

        public override void OnActEnd()
        {
            TurnOffTriggers();
            HTJ21GameManager.HouseController.UnlockDoors();
            HTJ21GameManager.HouseController.TurnOnLights();
            _showerController.Restart();
            _showerController.OnShowerFinish.RemoveListener(OnShowerFinished);
        }
    }
}
