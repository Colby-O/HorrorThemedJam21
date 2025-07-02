using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.DataPersistence;
using PlazmaGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

namespace HTJ21
{
    public class PrologueDirector : Director
    {
        [SerializeField] private List<GameObject> _items;

        [Header("Moon Cutscene")]
        [SerializeField] EventTrigger _moonLookTrigger;
        [SerializeField] private Transform _lookAtMoonLoc;
        [SerializeField] private AudioClip _moonJumpscareClip;
        [SerializeField] private float _pitch;
        [SerializeField] private float _turnDuration = 1f;
        [SerializeField] private float _lookAtMoonDuration;
        [SerializeField] protected float _timeBetweenCloseBlinkDuration;
        [SerializeField] protected float _timeBetweenOpenBlinkDuration;
        [SerializeField] private float _blinkDuration = 0.25f;
        [SerializeField] private float _scaleFactor;
        [SerializeField] private Light _redMoonLight;
        [SerializeField, ReadOnly] Vector3 _originalScale;

        [Header("Wake Up Cutscene")]
        [SerializeField] private Transform _cameraStart;
        [SerializeField] private float _curveWeight;
        [SerializeField] private float _cameraWaitDuration = 2f;
        [SerializeField] private float _cameraMoveDuration = 2f;
        [SerializeField, ReadOnly] private Vector3 _cameraEndPos;
        [SerializeField, ReadOnly] private Quaternion _cameraEndRot;

        [Header("Dialogues")]
        [SerializeField] private DialogueSO _introDialogue;
        [SerializeField] private DialogueSO _onWakeUpDialogue;
        [SerializeField] private DialogueSO _beforeMoonDialogue;
        [SerializeField] private DialogueSO _afterMoonDialogue;

        [Header("Object References")]
        [SerializeField] private MeshRenderer _moon;
        [SerializeField] private RadioController _radio;
        [SerializeField] private SafePad _safe;
        [SerializeField] private LampController _deskLamp;
        [SerializeField] private AlarmClock _clock;
        [SerializeField] private Portal _toAct1;
        [SerializeField] private Portal _atAct1;
        [SerializeField] private Portal _fromHomeMurder;
        [SerializeField] private Keypad _doorKeyPad;
        [SerializeField] private Door _bedroomDoor;
        [SerializeField] private Door _safeDoor;
        [SerializeField] private GameObject _mainRoadway;

        [SerializeField] private AudioSource _audioSource;

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

        private void EnableMoonEvent()
        {
            _moonLookTrigger.gameObject.SetActive(true);
        }

        public void WakeUpCutsceneLogic()
        {
            HTJ21GameManager.Player.LockMovement = true;

            Vector3 mid = Vector3.Lerp(_cameraStart.position, _cameraEndPos, 0.5f);
            Vector3 lineDir = (_cameraEndPos - _cameraStart.position).normalized;
            Vector3 perpendicular = Vector3.Cross(lineDir, -Vector3.right).normalized;
            BezierCurve curve = new BezierCurve(_cameraStart.position, mid + perpendicular *_curveWeight, _cameraEndPos);

            HTJ21GameManager.Player.GetCamera().transform.position = _cameraStart.position;
            HTJ21GameManager.Player.GetCamera().transform.rotation = _cameraStart.rotation;

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _cameraWaitDuration,
                (float t) =>
                {

                },
                () =>
                {
                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                        this,
                        _cameraMoveDuration,
                        (float t) =>
                        {
                            HTJ21GameManager.Player.GetCamera().transform.rotation = Quaternion.Lerp(_cameraStart.rotation, _cameraEndRot, t);
                            HTJ21GameManager.Player.GetCamera().transform.position = CurveUtility.EvaluatePosition(curve, t);
                        },
                        () =>
                        {
                            if (_onWakeUpDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_onWakeUpDialogue);
                            HTJ21GameManager.Player.LockMovement = false;
                        }
                    );
                }
            );
        }

        public void MoonJumpscare()
        {
            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().TriggerBlink(_blinkDuration, 0.5f, startFromOpen: true, onFinish: () =>
            {
                Vector3 targetScale = _moon.transform.localScale * _scaleFactor;
                HTJ21GameManager.Player.LockMovement = false;
                HTJ21GameManager.Player.LockMoving = true;
                HTJ21GameManager.Player.LookAt(_moon.transform);
                if (_radio) _radio.TurnOff();
                bool isLampOn = false;
                if (_deskLamp)
                {
                    isLampOn = _deskLamp.IsOn();
                    _deskLamp.TurnOff(true);
                }
                GameManager.GetMonoSystem<IWeatherMonoSystem>().DisableRain();
                GameManager.GetMonoSystem<IWeatherMonoSystem>().DisableThunder();
                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().SetScreenBend(0);
                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                    this,
                   _timeBetweenCloseBlinkDuration,
                    (float t) =>
                    {
                        _moon.transform.localScale = Vector3.Lerp(_originalScale, targetScale, t);
                        _moon.material.SetColor("_BaseColor", Color.Lerp(Color.white, HTJ21GameManager.Preferences.MoonRedColor, t));
                    },
                    () =>
                    {
                        _audioSource.pitch = _pitch;
                        _audioSource.PlayOneShot(_moonJumpscareClip);
                        GameManager.GetMonoSystem<IScreenEffectMonoSystem>().TriggerBlink(_blinkDuration, 0.5f, () => { GameManager.GetMonoSystem<IScreenEffectMonoSystem>().RestoreDefaults(); }, false);
                        _redMoonLight.gameObject.SetActive(true);
                        GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                            this,
                            _lookAtMoonDuration,
                            (float t) =>
                            {
                                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().SetStaticLevel(t);
                            },
                            () =>
                            {
                                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().TriggerBlink(_blinkDuration, 0.5f, () => { GameManager.GetMonoSystem<IScreenEffectMonoSystem>().SetScreenBend(0); }, true);
                                GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                                    this,
                                    _timeBetweenOpenBlinkDuration,
                                    (float t) =>
                                    {

                                    },
                                    () =>
                                    {
                                        _moon.transform.localScale = _originalScale;
                                        _moon.material.SetColor("_BaseColor", Color.white);
                                        _redMoonLight.gameObject.SetActive(false);

                                        GameManager.GetMonoSystem<IScreenEffectMonoSystem>().TriggerBlink(_blinkDuration, 0.5f, () =>
                                        {
                                            if (_afterMoonDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_afterMoonDialogue);
                                            GameManager.GetMonoSystem<IWeatherMonoSystem>().EnableRain();
                                            GameManager.GetMonoSystem<IWeatherMonoSystem>().EnableThunder();
                                            if (_radio) _radio.TurnOn();
                                            if (_deskLamp && isLampOn) _deskLamp.TurnOn(true);
                                            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().RestoreDefaults();
                                            _audioSource.pitch = 1f;
                                            HTJ21GameManager.Player.LockMoving = false;
                                            HTJ21GameManager.Player.StopLookAt();
                                        }, false);
                                    }
                                         );
                            }
                                 );
                    }
                         );
            });
        }

        private void MoonCutsceneLogic()
        {
            if (HTJ21GameManager.Inspector.IsInspecting()) HTJ21GameManager.Inspector.EndInspect();

            _originalScale = _moon.transform.localScale;
            Vector3 targetScale = _moon.transform.localScale * _scaleFactor;

            Vector3 startPos = HTJ21GameManager.Player.transform.position;
            Vector3 targetPos = _lookAtMoonLoc.position;
            targetPos.y = startPos.y;

            Quaternion curRot = HTJ21GameManager.Player.transform.rotation;
            Vector3 targetDirection = _moon.transform.position - HTJ21GameManager.Player.transform.position;
            Quaternion targetRot = Quaternion.LookRotation(targetDirection);

            HTJ21GameManager.Player.LockMovement = true;

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _turnDuration,
                (float t) =>
                {
                    HTJ21GameManager.Player.transform.position = Vector3.Lerp(startPos, targetPos, t);
                    HTJ21GameManager.Player.transform.rotation = Quaternion.Lerp(curRot, targetRot, t);
                },
                () =>
                {
                    if (_beforeMoonDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_beforeMoonDialogue);
                    else MoonJumpscare();
                }
            );
        }

        private void OpenPortals()
        {
            _mainRoadway.SetActive(true);
            _toAct1.Enable();
            _atAct1.Enable();
        }

        private void ClosePortals()
        {
            _toAct1.Disable();
            _atAct1.Disable();
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
            HTJ21GameManager.Player.LockMoving = false;
            HTJ21GameManager.Player.LockMovement = false;

            foreach (GameObject item in _items)
            {
                RestartInteractablesRecursive(item);
            }

            _doorKeyPad.Restart();
            _safe.Restart();
            _moonLookTrigger.Restart();
            _safeDoor.Restart();
            _bedroomDoor.Restart();
            _safeDoor.Lock();
            _bedroomDoor.Lock();

            _redMoonLight.gameObject.SetActive(false);
            _moon.material.SetColor("_BaseColor", Color.white);
            _moon.transform.localScale = _originalScale;
            _moon.gameObject.SetActive(true);

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

            _moonLookTrigger.gameObject.SetActive(false);
            _safe.OnSolved.AddListener(EnableMoonEvent);

            _doorKeyPad.OnSolved.AddListener(OpenPortals);

            _fromHomeMurder.Disable();
            ClosePortals();

            HTJ21GameManager.Player.LockMovement = true;
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.AddListener(OnPortalEnter);
           

            HTJ21GameManager.PickupManager.DropAll();
            HTJ21GameManager.Player.TurnOffLight();

            HTJ21GameManager.Inspector.EndInspect();

            HTJ21GameManager.Player.ResetPlayer();
            HTJ21GameManager.Player.ResetHead();
            HTJ21GameManager.Player.GetCamera().transform.position = _cameraStart.position;
            HTJ21GameManager.Player.GetCamera().transform.rotation = _cameraStart.rotation;
        }

        private void IntroMonologue()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().ShowLocation(
                $"Officer Graves House\nSnoqualmie, Washington\nJune 30th 2025\n{_clock.GetTime()}",
                () =>
                {
                    if (_introDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_introDialogue);
                }
            );
        }

        private void AddEvents()
        {
            GameManager.AddEventListener<Events.PrologueLookAtMoon>(Events.NewPrologueLookAtMoon((from, data) =>
            {
                MoonCutsceneLogic();
            }));
        }

        public override void OnActInit()
        {
            AddEvents();

            _originalScale = _moon.transform.localScale;

            _moonLookTrigger.gameObject.SetActive(false);

            _cameraEndPos = HTJ21GameManager.Player.GetCamera().transform.position;
            _cameraEndRot = HTJ21GameManager.Player.GetCamera().transform.rotation;

            _redMoonLight.gameObject.SetActive(false);
        }

        public override void OnActStart()
        {
            Setup();
            IntroMonologue();
        }

        private void OnPortalEnter(Portal p1, Portal p2)
        {
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        public override void OnActUpdate()
        {

        }

        public override void OnActEnd()
        {
            ClosePortals();
            _safe.OnSolved.RemoveListener(EnableMoonEvent);
            _doorKeyPad.OnSolved.RemoveListener(OpenPortals);
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.RemoveListener(OnPortalEnter);
            _moonLookTrigger.gameObject.SetActive(false);
            _moon.gameObject.SetActive(false);
            _redMoonLight.gameObject.SetActive(false);
        }
    }
}
