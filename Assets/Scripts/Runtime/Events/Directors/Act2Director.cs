using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using static UnityEngine.GraphicsBuffer;

namespace HTJ21
{
    public class PrologueDirector : MonoBehaviour
    {
        [Header("Moon Cutscene")]
        [SerializeField] EventTrigger _moonLookTrigger;
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

        [Header("Object References")]
        [SerializeField] private MeshRenderer _moon;
        [SerializeField] private RadioController _radio;
        [SerializeField] private SafePad _safe;

        [SerializeField] private AudioSource _audioSource;


        private void EnableMoonEvent()
        {
            _moonLookTrigger.gameObject.SetActive(true);
        }

        private void WakeUpCutsceneLogic()
        {
            HTJ21GameManager.Player.LockMovement = true;
            _cameraEndPos = HTJ21GameManager.Player.GetCamera().transform.position;
            _cameraEndRot = HTJ21GameManager.Player.GetCamera().transform.rotation;

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
                            HTJ21GameManager.Player.LockMovement = false;
                        }
                    );
                }
            );
        }

        private void MoonCutsceneLogic()
        {
            _originalScale = _moon.transform.localScale;
            Vector3 targetScale = _moon.transform.localScale * _scaleFactor;

            Quaternion curRot = HTJ21GameManager.Player.transform.rotation;
            Vector3 targetDirection = _moon.transform.position - HTJ21GameManager.Player.transform.position;
            Quaternion targetRot = Quaternion.LookRotation(targetDirection);

            HTJ21GameManager.Player.LockMovement = true;

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _turnDuration,
                (float t) =>
                {
                    HTJ21GameManager.Player.transform.rotation = Quaternion.Lerp(curRot, targetRot, t);
                },
                () =>
                {
                    GameManager.GetMonoSystem<IScreenEffectMonoSystem>().TriggerBlink(_blinkDuration, 0.5f, startFromOpen: true, onFinish: () =>
                    {
                        HTJ21GameManager.Player.LockMovement = false;
                        HTJ21GameManager.Player.LockMoving = true;
                        HTJ21GameManager.Player.LookAt(_moon.transform);
                        _radio.TurnOff();
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
                                                    _radio.TurnOn();
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
            );
        }

        private void StartAct()
        {
            _redMoonLight.gameObject.SetActive(false);
            _moon.material.SetColor("_BaseColor", Color.white);

            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player) player.EnablePlayer();

            GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
            HTJ21GameManager.IsPaused = false;

            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SkipLocation();
            GameManager.GetMonoSystem<IDialogueMonoSystem>().ResetDialogue();
            GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOff();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().DisableThunder();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().DisableRain();

            _moonLookTrigger.gameObject.SetActive(false);
            _safe.OnSolved.AddListener(EnableMoonEvent);

            AddEvents();
        }

        private void AddEvents()
        {
            GameManager.AddEventListener<Events.PrologueLookAtMoon>(Events.NewPrologueLookAtMoon((from, data) =>
            {
                MoonCutsceneLogic();
            }));
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
           
        }

        private void Start()
        {
            StartAct();
            WakeUpCutsceneLogic();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }
    }
}
