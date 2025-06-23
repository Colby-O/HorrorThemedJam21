using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class Home1Director : Director
    {
        [Header("Fake Jumpscare")]
        [SerializeField] private EventTrigger _fakeScareTrigger;
        [SerializeField] private float _lightOffDuration;
        [SerializeField] private AudioClip _tensionClip;
        

        [Header("Real Jumpscare")]
        [SerializeField] private EventTrigger _realScareTrigger;
        [SerializeField] private float _numFlickersReal;
        [SerializeField] private AudioClip _buildUp1;
        [SerializeField] private AudioClip _buildUp2;
        [SerializeField] private float _waitDuration;
        [SerializeField] private AudioClip _jumpscareTrack;
        [SerializeField] private float _scareDuration;

        [Header("Light Flickering")]
        [SerializeField] private bool _isFlickering;
        [SerializeField] private float _flickerRate = 1;
        [SerializeField] private float _timer = 0f;
        [SerializeField, ReadOnly] bool _isOn;

        [Header("Refereces")]
        [SerializeField] private GameObject _jumpscare;

        private void StartFlickering()
        {
            HTJ21GameManager.HouseController.TurnOffAllLights();
            _isFlickering = true;
            _timer = 0f;
            _isOn = false;
        }

        private void StopFlickering()
        {
            HTJ21GameManager.HouseController.TurnOnLights();
            _isFlickering = false;
            _timer = 0f;
            _isOn = false;
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
                }
            );
        }

        private void RealScare()
        {
            StopFlickering();
            HTJ21GameManager.HouseController.TurnOffAllLights();
            HTJ21GameManager.HouseController.LockAllDoors();
            HTJ21GameManager.Player.LockMoving = true;
            HTJ21GameManager.Player.LookAt(_jumpscare.transform);

            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_buildUp1, PlazmaGames.Audio.AudioType.Music, false, false);

            bool isOn = false;
            float step = 0;
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _buildUp1.length,
                (float t) =>
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
                },
                () =>
                {
                    GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                    GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_buildUp2, PlazmaGames.Audio.AudioType.Music, false, false);
                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                        this,
                        _buildUp2.length,
                        (float t) =>
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
                        },
                        () =>
                        {
                            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                this,
                                _waitDuration,
                                (float t) =>
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
                                },
                                () =>
                                {
                                    GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                                    GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_jumpscareTrack, PlazmaGames.Audio.AudioType.Music, false, false);
                                    _jumpscare.gameObject.SetActive(true);
                                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                        this,
                                        _jumpscareTrack.length,
                                        (float t) =>
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
                                        },
                                        () =>
                                        {
                                            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
                                            HTJ21GameManager.Player.LockMoving = false;
                                            HTJ21GameManager.Player.StopLookAt();
                                            HTJ21GameManager.HouseController.TurnOnLights();
                                            HTJ21GameManager.HouseController.UnlockDoors();
                                            _jumpscare.gameObject.SetActive(false);
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
            GameManager.AddEventListener<Events.Home1FakeScare>(Events.NewHome1FakeScare((from, data) =>
            {
                FakeScare();
            }));

            GameManager.AddEventListener<Events.Home1RealScare>(Events.NewHome1RealScare((from, data) =>
            {
                RealScare();
            }));
        }

        private void Setup()
        {
            GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOff();

            HTJ21GameManager.Player.DisableFlashlight();

            _fakeScareTrigger.gameObject.SetActive(true);
            _realScareTrigger.gameObject.SetActive(true);

        }

        public override void OnActInit()
        {
            _fakeScareTrigger.gameObject.SetActive(false);
            _realScareTrigger.gameObject.SetActive(false);
            _jumpscare.SetActive(false);
        }

        public override void OnActStart()
        {
            Setup();
            AddEvents();
        }

        public override void OnActUpdate()
        {
            if (_isFlickering)
            {
                Debug.Log("Flickering");
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
            HTJ21GameManager.Player.EnableFlashlight();
        }
    }
}
