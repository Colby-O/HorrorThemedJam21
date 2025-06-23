using PlazmaGames.Animation;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class Home1Director : Director
    {
        [Header("Fake Jumpscare")]
        [SerializeField] private EventTrigger _fakeScareTrigger;
        [SerializeField] private float _lightOffDuration;
        [SerializeField] private float _lightFlickerDuration;
        [SerializeField] private float _numFlickersFake;

        [Header("Real Jumpscare")]
        [SerializeField] private EventTrigger _realScareTrigger;
        [SerializeField] private float _scareDuration;
        [SerializeField] private float _numFlickersReal;

        [Header("Refereces")]
        [SerializeField] private GameObject _jumpscare;

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
                    bool isOn = false;
                    float step = 0;
                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                        this,
                        _lightFlickerDuration,
                        (float t) =>
                        {
                            if (_numFlickersFake > 0 && t > step)
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
                                step += 1f / (float)_numFlickersFake;
                            }
                        },
                        () =>
                        {
                            HTJ21GameManager.HouseController.TurnOnLights();
                            HTJ21GameManager.HouseController.UnlockDoors();
                        }
                    );
                }
            );
        }

        private void RealScare()
        {
            _jumpscare.gameObject.SetActive(true);
            HTJ21GameManager.HouseController.TurnOffAllLights();
            HTJ21GameManager.HouseController.LockAllDoors();
            HTJ21GameManager.Player.LockMoving = true;
            HTJ21GameManager.Player.LookAt(_jumpscare.transform);

            bool isOn = false;
            float step = 0;
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _lightOffDuration,
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
                    HTJ21GameManager.Player.LockMoving = false;
                    HTJ21GameManager.Player.StopLookAt();
                    HTJ21GameManager.HouseController.TurnOnLights();
                    HTJ21GameManager.HouseController.UnlockDoors();
                    _jumpscare.gameObject.SetActive(false);
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

        }

        public override void OnActEnd()
        {
            HTJ21GameManager.Player.EnableFlashlight();
        }
    }
}
