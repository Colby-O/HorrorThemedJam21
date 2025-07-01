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

        [Header("Moon")]
        [SerializeField] private MeshRenderer _moon;
        [SerializeField] private Vector3 _moonSize;

        [Header("Music")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private float _musicVolume = 1f;
        [SerializeField] private float _fadeTime = 5f;

        [Header("Dialogue")]
        [SerializeField] private DialogueSO _startDialogue;
        [SerializeField] private DialogueSO _endDialogue;

        [Header("Refereces")]
        [SerializeField] private EventTrigger _musicChangeTrigger;
        [SerializeField] private GameObject _jumpscare;
        [SerializeField] private Transform _startLoc;
        [SerializeField] private ItemPickup _bathroomSupplies;
        [SerializeField] private Door _bathroomDoor;
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
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _lightOffDuration,
                (float t) =>
                {

                },
                () =>
                {
                    StartFlickering();
                    GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                    GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_tensionClip, PlazmaGames.Audio.AudioType.Music, true, false);
                    _isClam = true;
                    _musicChangeTrigger.gameObject.SetActive(true);
                    _realScareTrigger.gameObject.SetActive(true);
                    _showerController.Disable();
                    //StopAllMusic(false);
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
                            Quaternion curRot = HTJ21GameManager.Player.transform.rotation;
                            Vector3 targetDirection = _jumpscare.transform.position - HTJ21GameManager.Player.transform.position;
                            Quaternion targetRot = Quaternion.LookRotation(targetDirection);

                            HTJ21GameManager.Player.LockMovement = true;

                            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                this,
                                0.1f,
                                (float t) =>
                                {
                                    HTJ21GameManager.Player.transform.rotation = QuaternionExtension.Lerp(curRot, targetRot, t, true);
                                },
                                () =>
                                {
                                    HTJ21GameManager.Player.LockMovement = false;
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
                                            //GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(this, _fadeTime, (float t) => AudioHelper.FadeIn(_musicSource, 0f, _musicVolume, t));
                                            HTJ21GameManager.Player.LockMoving = false;
                                            HTJ21GameManager.Player.StopLookAt();
                                            HTJ21GameManager.HouseController.TurnOnLights();
                                            HTJ21GameManager.HouseController.UnlockDoors();
                                            StopFlickering();
                                            _showerController.Enable();
                                            _jumpscare.gameObject.SetActive(false);
                                            _musicChangeTrigger.gameObject.SetActive(false);
                                            _showerController.Enable();
                                            if (_endDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_endDialogue);
                                        }
                                    );
                                }
                            );
                        }
                    );
                }
            );
        }

        private void AddEvents()
        {
            _bathroomSupplies.OnPickupCallback.AddListener(FakeScare);

            GameManager.AddEventListener<Events.Home1RealScare>(Events.NewHome1RealScare((from, data) =>
            {
                RealScare();
            }));
            GameManager.AddEventListener<Events.Home1ToggleMusic>(Events.NewHome1ToggleMusic((from, data) =>
            {
                GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_buildUp1, PlazmaGames.Audio.AudioType.Music, false, false);
                _isClam = false;

                _bathroomDoor.Close(false, true);
                _bathroomDoor.Lock();
            }));
        }

        private void OnShowerFinished()
        {
            _showerController.RestoreToDefaults();
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        private void TurnOffTriggers()
        {
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
            _bathroomSupplies.Restart();

            _bathroomDoor.Restart();
            _bathroomDoor.Unlock();

            foreach (GameObject item in _items)
            {
                RestartInteractablesRecursive(item);
            }

            _showerController.Enable();
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

            if (_startDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_startDialogue);

            HTJ21GameManager.HouseController.UnlockDoors();
            HTJ21GameManager.HouseController.TurnOnLights();

            _moon.gameObject.SetActive(true);
            _moon.material.SetColor("_BaseColor", HTJ21GameManager.Preferences.MoonRedColor);
            _moon.transform.localScale = _moonSize;

            _realScareTrigger.gameObject.SetActive(false);
            _musicChangeTrigger.gameObject.SetActive(false);
            _isFlickering = false;

            _tvObjects.SetActive(true);
            _tvCamera.SetScreen(_tvScreen);

            StopAllMusic(true);
            float volScale = GameManager.GetMonoSystem<IAudioMonoSystem>().GetOverallVolume() * GameManager.GetMonoSystem<IAudioMonoSystem>().GetMusicVolume();
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(this, _fadeTime, (float t) => AudioHelper.FadeIn(_musicSource, 0f, _musicVolume * volScale, t));

            _realScareTrigger.Restart();
            _musicChangeTrigger.Restart();
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

        public override void OnActInit()
        {
            TurnOffTriggers();
            AddEvents();
            _tvObjects.SetActive(false);
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
            StopAllMusic(false);
            _tvObjects.SetActive(false);
            _tvCamera.SetScreen(null);
            _moon.gameObject.SetActive(false);
            TurnOffTriggers();
            HTJ21GameManager.HouseController.UnlockDoors();
            HTJ21GameManager.HouseController.TurnOnLights();
            _showerController.Restart();
            _showerController.OnShowerFinish.RemoveListener(OnShowerFinished);
        }
    }
}
