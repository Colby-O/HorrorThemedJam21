using System.Collections.Generic;
using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.Core.Utils;
using PlazmaGames.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using Time = UnityEngine.Time;

namespace HTJ21
{
    public class EpilogueDirector : Director
    {
        private static readonly int MatBaseColorId = Shader.PropertyToID("_BaseColor");
        
        [SerializeField] private List<GameObject> _items;

        [SerializeField] private Transform _playerStartLocation;
        [SerializeField] private Transform _carStartLocation;

        [SerializeField] private Transform[] _cultists;
        [SerializeField] private Vector3[] _cultistsStartPos;
        [SerializeField] private Quaternion[] _cultistsStartRot;
        [SerializeField] private Transform[] _torches;
        
        [SerializeField] private MeshRenderer _cultCover;
        [SerializeField] private Transform _ritualCenter;
        [SerializeField] private Transform _child;
        [SerializeField] private CultAnimator _axeSwinger;
        [SerializeField] private Transform _axeSwingerTemp;
        [SerializeField] private Transform _axeSwingerEndPos;
        
        [SerializeField] private Transform _nathanHead;
        [SerializeField] private Transform _nathanRealHead;
        [SerializeField] private Vector3 _headChopForce = Vector3.right;
        [SerializeField] private float _headWaitTime = 5;
        
        [SerializeField] private float _revealCultFadeTime = 2.0f;

        [SerializeField] private List<float> _splineSpeeds = new();
        [SerializeField] private Transform _cineCamera;
        [SerializeField] private Camera _cineCameraCam;
        [SerializeField] private SplineContainer _cineSplines;

        private Vector3 _childBodyStartPosition;
        private Quaternion _childBodyStartRotation;
        
        private float _revealCultStartTime = 0;

        private bool _atBloodTrail = false;
        private bool _turnedOffCarLights = false;
        
        private int _currentSpline = -1;
        private float _splineStartTime = 0;
        private float _centerToChildStartTime = 0;
        private Vector3 _startRitualCenter;

        private float _axerWalkStartTime = 0;
        private Vector3 _axeSwingerStartPos;
        private float _headWaitStartTime = 0;
        private bool _movingMoon = false;
        private float _moveMoveStartTime = 0;
        [SerializeField] private Transform _childBody;
        [SerializeField] private Transform _childBodyPivot;
        [SerializeField] private Transform _moon;
        [SerializeField] private float _moonSizeScale = 1.8f;
        [SerializeField] private float _moonApproachSpeed = 5;
        [SerializeField] private float _moonLerpExp = 3;
        [SerializeField] private Transform _moonTarget;
        [SerializeField] private float _thunderLifespan = 1;
        [SerializeField] private float _thunderInterval = 1.5f;
        private Vector3 _moonStartPosition;
        private Vector3 _moonStartScale;
        private Material _moonMaterial;
        private IWeatherMonoSystem _weather;
        private bool _thundering = false;
        private float _lastThunder = 0;

        private bool _movingChild = false;

        private bool _startCinema = false;

        [SerializeField] private float _moonFadeBackTime = 5;

        private float _moonFadeBackStartTime = 0;
        private float _moveChopperBackStartTime = 0;
        [SerializeField] private float _childRiseSpeed = 0.5f;
        [SerializeField] private float _childRotateSpeed = 30;
        private float _moveMoonBackStartTime = 0;
        private bool _stageMoon = false;
        private Levitate _moonLevitate;
        private float _cultistsRunawayStartTime = 0;

        [SerializeField] private Transform _cultRunawayDir;
        [SerializeField] private float _cultistsRunawayTime = 4;
        [SerializeField] private float _cultistsRunawaySpeed = 0.6f;
        private bool _playerMovingToCenter = false;
        [SerializeField] private float _playerApproachSpeed = 0.5f;
        
        private bool _saveChild = false;
        [SerializeField] private float _saveChildFadeToBlackTime = 3.5f;
        
        [SerializeField] private Transform _endCar;
        [SerializeField] private Transform _endCarHeadPosition;
        [SerializeField] private float _endCarMoveDuration = 10.0f;
        [SerializeField] private SplineContainer _endCarSpline;
        [SerializeField] private float _endCarRightOffset = 1.5f;
        [SerializeField] private Light _sunLight;
        private float _moveEndCarStartTime = 0;
        
        private Dictionary<string, DialogueSO> _dialogues = new();
        [SerializeField] private Transform _moon2;
        [SerializeField] private float _moonAttackTime = 22;
        [SerializeField] private float _childLookTime = 8;
        [SerializeField] private float _childLookDuration = 5;
        private float _childLookStartTime = 0;
        private float _doChildLookStartTime = 0;
        [SerializeField] private float _headTurnRatioSpeed = 3;
        [SerializeField] private float _moonMoveUpTime = 1.25f;
        [SerializeField] private Transform _moon2Target;
        private Vector3 _moon2StartPos;
        private bool _moonGrowing = false;
        private float _moonGrowingStartTime = 0;
        [SerializeField] private float _moonScaleSpeed = 0.2f;
        [SerializeField] private float _fullStartTime = 5;
        [SerializeField] private float _maxStatic = 1;
        [SerializeField] private Color _skyColor = new Color(0.6f,0.6f, 1.0f);
        [SerializeField] private Color _redSkyColor = new Color(0.7f, 0.4f, 0.4f);
        [SerializeField] private float _finalFadeToBlackTime = 4;
        [SerializeField] private Transform[] _tress;
        private Vector3 _nhStartPos;
        private Vector3 _realMoonStartScale;
        private Quaternion _nhStartRot;
        private bool _loop = false;
        [SerializeField] private Transform _loopPosition;
        private Vector3 _moon2StartScale;

        private void LoadDialogue(string name)
        {
            _dialogues.Add(name, Resources.Load<DialogueSO>($"Dialogue/Epilogue/{name}"));
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
        
        public override void OnActInit()
        {
            _cultistsStartRot = new Quaternion[_cultists.Length];
            _cultistsStartPos = new Vector3[_cultists.Length];
            for (int i = 0; i < _cultists.Length; i++) _cultistsStartRot[i] = _cultists[i].rotation;
            for (int i = 0; i < _cultists.Length; i++) _cultistsStartPos[i] = _cultists[i].position;
            _childBodyStartPosition = _childBodyPivot.position;
            _childBodyStartRotation = _childBodyPivot.rotation;
            _nhStartPos = _nathanHead.position;
            _nhStartRot = _nathanHead.rotation;
            _cineCameraCam = _cineCamera.GetComponent<Camera>();
            _moon2StartPos = _moon2.position;
            _moon2StartScale = _moon2.localScale;
            _realMoonStartScale = _moon.localScale;
            LoadDialogue("SaveChild");
            LoadDialogue("End");
            
            _moon2.GetComponent<MeshRenderer>().material.SetColor(MatBaseColorId, HTJ21GameManager.Preferences.MoonRedColor);
            _moonLevitate = _moon.GetComponent<Levitate>();
            _weather = GameManager.GetMonoSystem<IWeatherMonoSystem>();
            _moonMaterial = _moon.GetComponent<MeshRenderer>().material;
            _axeSwinger.OnHalfFinish.AddListener(ChopOffHead);
            _axeSwinger.OnFinish.AddListener(MoveCultChopperBack);
            _axeSwingerStartPos = _axeSwingerTemp.position;
            _startRitualCenter = _ritualCenter.position;
            _cineCamera.gameObject.SetActive(false);
            GameManager.AddEventListener<Events.CultCutscene>(Events.NewCultCutscene((from, data) =>
            {
                _startCinema = true;
                HTJ21GameManager.Player.LockMovement = true;
            }));
            GameManager.AddEventListener<Events.End>(Events.NewEnd((from, data) =>
            {
                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().FadeToBlack(_finalFadeToBlackTime, () =>
                {
                    Screen.fullScreen = false;
                    Application.Quit();
                });
            }));
            GameManager.AddEventListener<Events.GotoFinalCarScene>(Events.NewGotoFinalCarScene((from, data) =>
            {
                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().FadeToBlack(_saveChildFadeToBlackTime, () =>
                {
                    _endCar.gameObject.SetActive(true);
                    _cineCamera.parent = _endCarHeadPosition;
                    _cineCamera.localPosition = Vector3.zero;
                    _cineCamera.localRotation = Quaternion.identity;

                    _moveEndCarStartTime = Time.time;
                    _sunLight.gameObject.SetActive(true);
                    _moon.gameObject.SetActive(false);
                    _moon2.gameObject.SetActive(true);

                    _cineCamera.GetComponent<Camera>().backgroundColor = _skyColor;

                    _childLookStartTime = Time.time;
                    GameManager.GetMonoSystem<IScreenEffectMonoSystem>().UnfadeToBlack(_saveChildFadeToBlackTime, () =>
                    {
                        
                    });
                });
            }));
            GameManager.AddEventListener<Events.RevealCult>(Events.NewRevealCult((from, data) =>
            {
                if (!_loop) _revealCultStartTime = Time.time;
            }));
            GameManager.AddEventListener<Events.AtBloodTrail>(Events.NewAtBloodTrail((from, data) =>
            {
                _atBloodTrail = true;
            }));
        }

        private void MoveCultChopperBack()
        {
            _moveChopperBackStartTime = Time.time;
            _axeSwinger.gameObject.SetActive(false);
            _axeSwingerTemp.gameObject.SetActive(true);
        }

        private void ChopOffHead()
        {
            _nathanHead.gameObject.SetActive(true);
            _nathanHead.GetComponent<Rigidbody>().AddForce(_headChopForce, ForceMode.Impulse);
            _nathanRealHead.localScale = Vector3.zero;

            _headWaitStartTime = Time.time;
        }

        public override void OnActStart()
        {
            _saveChild = false;
            _moon.gameObject.SetActive(true);
            _moon2.position = _moon2StartPos;
            _moon2.localScale = _moon2StartScale;
            _cineCameraCam.backgroundColor = Color.black;
            _sunLight.gameObject.SetActive(false);
            for (int i = 0; i < _cultists.Length; i++) _cultists[i].rotation = _cultistsStartRot[i];
            for (int i = 0; i < _cultists.Length; i++) _cultists[i].position = _cultistsStartPos[i];
            for (int i = 0; i < _cultists.Length; i++) _cultists[i].gameObject.SetActive(true);
            _childBodyPivot.gameObject.SetActive(true);
            _childBodyPivot.position = _childBodyStartPosition;
            _childBodyPivot.rotation = _childBodyStartRotation;
            _moon.localScale = _realMoonStartScale;
            _nathanHead.gameObject.SetActive(false);
            _nathanHead.position = _nhStartPos;
            _nathanHead.rotation = _nhStartRot;
            _nathanHead.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            _nathanRealHead.localScale = Vector3.one;
            _tress.ForEach(t => t.localEulerAngles = t.localEulerAngles.SetZ(0));
            transform.parent.GetComponentsInChildren<EventTrigger>().ForEach(e => e.Restart());
            transform.parent.GetComponentsInChildren<TreeFallOnTrigger>().ForEach(e => e.Restart());
            _moon.transform.localScale *= _moonSizeScale;
            _moonMaterial.SetColor(MatBaseColorId, HTJ21GameManager.Preferences.MoonRedColor);
            _nathanHead.gameObject.SetActive(false);
            _cultCover.gameObject.SetActive(true);
            _cultCover.material.color = _cultCover.material.color.SetA(1);
            _cineCamera.gameObject.SetActive(false);
            HTJ21GameManager.Car.SetDrivingProfile("Emergency");
            
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Player);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Car);
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(HTJ21GameManager.Inspector);

            HTJ21GameManager.Car.RestartHitch();
            HTJ21GameManager.Car.Restart();
            HTJ21GameManager.Car.ExitCar(true);
            HTJ21GameManager.Car.DisableMirrors();
            HTJ21GameManager.Car.Unlock();
            HTJ21GameManager.Car.transform.Find("AtAct1").GetComponent<Portal>().Disable();
            HTJ21GameManager.CinematicCar.Disable();

            HTJ21GameManager.Player.EnablePlayer();
            HTJ21GameManager.Player.StopLookAt();
            HTJ21GameManager.Player.ResetHead();
            HTJ21GameManager.Player.ResetCamera();
            HTJ21GameManager.Player.Teleport(_playerStartLocation.position);
            HTJ21GameManager.Car.Teleport(_carStartLocation.position, _carStartLocation.rotation);
            HTJ21GameManager.Player.LockMoving = false;
            HTJ21GameManager.Player.LockMovement = false;
            HTJ21GameManager.Inspector.EndInspect();

            HTJ21GameManager.PickupManager.DropAll();
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.FlashLight);
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.BathroomKey);
            HTJ21GameManager.PickupManager.Pickup(PickupableItem.BathroomSupplies);

            foreach (GameObject item in _items)
            {
                RestartInteractablesRecursive(item);
            }

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
            
            HTJ21GameManager.Player.GetCamera().gameObject.SetActive(true);

            if (_loop)
            {
                _revealCultStartTime = Time.time;
                HTJ21GameManager.Player.Teleport(_loopPosition.position);
                HTJ21GameManager.Player.transform.localEulerAngles = HTJ21GameManager.Player.transform.localEulerAngles.SetY(_loopPosition.localEulerAngles.y);
            }
        }

        private void StartCinema()
        {
            HTJ21GameManager.Player.GetCamera().gameObject.SetActive(false);
            _cineCamera.gameObject.SetActive(true);
            _cineCamera.position = HTJ21GameManager.Player.GetCamera().transform.position;
            _cineCamera.rotation = HTJ21GameManager.Player.GetCamera().transform.rotation;
            BezierKnot k = _cineSplines[0][0];
            k.Position = _cineCamera.position;
            k.Rotation = _cineCamera.rotation;
            _cineSplines[0].SetKnot(0, k);
            _currentSpline = 0;
            _splineStartTime = Time.time;
        }

        private void FadeoutCultCover()
        {
            if (_revealCultStartTime == 0) return;
            
            float t = (Time.time - _revealCultStartTime) / _revealCultFadeTime;
            if (t >= 1)
            {
                _revealCultStartTime = 0;
                t = 1;
            }

            _cultCover.material.color = _cultCover.material.color.SetA(1.0f - t);
        }

        public override void OnActUpdate()
        {
            MoonGrow();
            MoonMoveUp();
            MoveEndCar();
            if (_playerMovingToCenter)
            {
                float dist = 1.0f;
                if (_saveChild) dist = 3.0f;
                if (Vector3.Distance(HTJ21GameManager.Player.transform.position, _ritualCenter.position) < dist)
                {
                    _playerMovingToCenter = false;
                    HTJ21GameManager.Player.LookAt(_moon);
                    HTJ21GameManager.Player.UncontrollableApproach = 0;
                    HTJ21GameManager.Player.LockMovement = true;

                    if (_saveChild)
                    {
                        GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_dialogues["SaveChild"]);
                    }
                    else
                    {
                        GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_dialogues["End"]);
                    }
                }
            }
            CultistsRunAway();
            if (_stageMoon && !_moonLevitate.IsLevitating())
            {
                _stageMoon = false;
                StartMoveMoonBack();
            }
            DoChildLook();
            ChildLook();
            MoveMoonBack();
            MoveChild();
            FadeMoon();
            MoveChopperBack();
            if (_startCinema)
            {
                _startCinema = false;
                StartCinema();
            }
            if (_headWaitStartTime > 0 && Time.time - _headWaitStartTime >= _headWaitTime)
            {
                _headWaitStartTime = 0;
                _currentSpline = 2;
                BezierKnot k = _cineSplines[_currentSpline][0];
                k.Position = _cineCamera.position;
                k.Rotation = _cineCamera.rotation;
                _cineSplines[_currentSpline][0] = k;
                _splineStartTime = Time.time;
            }

            MoveMoon();
            FadeoutCultCover();
            CenterToChild();
            if (!_turnedOffCarLights && _atBloodTrail && !HTJ21GameManager.Player.IsInCar())
            {
                _turnedOffCarLights = true;
                HTJ21GameManager.Car.TurnOffLights();
            }

            MoveSpline();
            AxerWalk();
        }

        private void MoonGrow()
        {
            if (!_moonGrowing) return;

            _moon2.localScale += Vector3.one * (Time.deltaTime * _moonScaleSpeed);
            float t = (Time.time - _moonGrowingStartTime) / _fullStartTime;
            if (t >= 1)
            {
                t = 1;
                _moonGrowing = false;
                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().FadeToBlack(_finalFadeToBlackTime, () =>
                {
                    _loop = true;
                    GameManager.GetMonoSystem<IDirectorMonoSystem>().StartAct(Act.Epilogue);
                });
                
            }
            
            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().SetStaticLevel(t * _maxStatic);
            _cineCameraCam.backgroundColor = Color.Lerp(_skyColor, _redSkyColor, t);

        }

        private void MoonMoveUp()
        {
            if (_moveEndCarStartTime == 0) return;
            float t = (Time.time - _moveEndCarStartTime) / _endCarMoveDuration;
            if (t >= 1.0f)
            {
                t = 1.0f;
                _moonGrowing = true;
                _moonGrowingStartTime = Time.time;
            }

            _moon2.position = Vector3.Lerp(_moon2StartPos, _moon2Target.position, t);
        }

        private void DoChildLook()
        {
            if (_doChildLookStartTime == 0) return;
            
            float t = (Time.time - _doChildLookStartTime) / _childLookDuration;
            if (t >= 1.0f)
            {
                t = 1.0f;
                _doChildLookStartTime = 0;
            }

            if (t < _headTurnRatioSpeed)
            {
                float lt = t / _headTurnRatioSpeed;
                _cineCamera.localEulerAngles = _cineCamera.localEulerAngles.SetY(Mathf.Lerp(0, 65, lt));
            }

            if (t + _headTurnRatioSpeed >= 1.0f)
            {
                float lt = (t - (1.0f - _headTurnRatioSpeed)) / _headTurnRatioSpeed;
                _cineCamera.localEulerAngles = _cineCamera.localEulerAngles.SetY(Mathf.Lerp(65, 0, lt));
            }


            if (t >= 1.0f)
            {
                
            }
        }
        
        private void ChildLook()
        {
            if (_childLookStartTime == 0) return;
            
            float t = (Time.time - _childLookStartTime) / _childLookTime;
            if (t >= 1.0f)
            {
                t = 1.0f;
                _childLookStartTime = 0;
                _doChildLookStartTime = Time.time;
            }
        }


        private void MoveEndCar()
        {
            if (_moveEndCarStartTime == 0) return;
            float t = (Time.time - _moveEndCarStartTime) / _endCarMoveDuration;
            if (t >= 1.0f) t = 1.0f;

            Unity.Mathematics.float3 position, tangent, up;
            if (_endCarSpline.Evaluate(0, t, out position, out tangent, out up))
            {
                Vector3 right = Vector3.Cross(tangent, up).normalized;
                _endCar.position = (Vector3)position + right * _endCarRightOffset;
                _endCar.rotation = Quaternion.LookRotation(Vector3.Normalize(tangent), Vector3.Normalize(up));
            }

            if (t >= 1.0f)
            {
                _moveEndCarStartTime = 0;
            }
        }

        private void CultistsRunAway()
        {
            if (_cultistsRunawayStartTime == 0) return;
            
            float t = (Time.time - _cultistsRunawayStartTime) / _cultistsRunawayTime;

            if (t >= 1)
            {
                _cultistsRunawayStartTime = 0;
                foreach (Transform c in _cultists)
                {
                    c.gameObject.SetActive(false);
                }
            }

            foreach (Transform c in _cultists)
            {
                c.position += _cultRunawayDir.forward * (_cultistsRunawaySpeed * Time.deltaTime);
                c.localEulerAngles = c.localEulerAngles.SetY(_cultRunawayDir.localEulerAngles.y + 180);
            }
        }

        private void MoveMoonBack()
        {
            if (_moveMoonBackStartTime == 0) return;
            float t = (Time.time - _moveMoonBackStartTime) / _moonApproachSpeed;

            if (t >= 1)
            {
                t = 1;
                _moveMoonBackStartTime = 0;
                _cultistsRunawayStartTime = Time.time;
                _playerMovingToCenter = true;
                HTJ21GameManager.Player.LookAt(_ritualCenter);
                HTJ21GameManager.Player.UncontrollableApproach = _playerApproachSpeed;
                HTJ21GameManager.Player.LockMovement = false;
            }
            
            float expT = Mathf.Pow(t, _moonLerpExp);
            _moon.transform.position = Vector3.Lerp(_moonTarget.position, _moonStartPosition, expT);
            _moon.transform.localScale = Vector3.Lerp(_moonTarget.localScale, _moonStartScale, expT);
        }

        private void MoveChild()
        {
            if (!_movingChild) return;
            _childBodyPivot.position = _childBodyPivot.position.AddY(Time.deltaTime * _childRiseSpeed);
            _childBodyPivot.localEulerAngles = _childBodyPivot.localEulerAngles.AddY(Time.deltaTime * _childRotateSpeed);
            if (Vector3.Distance(_childBodyPivot.position, _moon.position) < 0.25f)
            {
                _childBodyPivot.gameObject.SetActive(false);
                _thundering = false;
                _movingChild = false;

                _moon.GetComponent<Levitate>().StopLevitatingAtNextBase();
                _stageMoon = true;
            }
        }

        private void StartMoveMoonBack()
        {
            _moon.Find("Light").gameObject.SetActive(false);
            _moveMoonBackStartTime = Time.time;
            foreach (Transform torch in _torches)
            {
                torch.Find("Light").gameObject.SetActive(true);
                torch.Find("Particle System").gameObject.SetActive(true);
                MeshRenderer mr = torch.Find("Model").GetComponent<MeshRenderer>();
                mr.materials.ForEach(m => m.SetInt("Boolean_8BBF99CD", 0));
            }
        }

        private void MoveChopperBack()
        {
            if (_moveChopperBackStartTime == 0) return;
            float t = (Time.time - _moveChopperBackStartTime) / 1;
            _axeSwingerTemp.transform.position = Vector3.Lerp(_axeSwingerEndPos.position, _axeSwingerStartPos, t);
            
            if (t >= 1)
            {
                t = 1;
                _moveChopperBackStartTime = 0;
            }
        }

        private void FadeMoon()
        {
            if (_moonFadeBackStartTime == 0) return;
            float t = (Time.time - _moonFadeBackStartTime) / _moonFadeBackTime;
            if (t >= 1)
            {
                t = 1;
                _moonFadeBackStartTime = 0;
                _movingChild = true;
            }
            _moonMaterial.SetColor(MatBaseColorId, Color.Lerp(HTJ21GameManager.Preferences.MoonRedColor, Color.white, t));
        }

        private void MoveMoon()
        {
            if (_thundering && Time.time - _lastThunder >= _thunderInterval)
            {
                _lastThunder = Time.time;
                _weather.SpawnLightingAt(_ritualCenter.position);
            }
            
            if (_movingMoon)
            {
                float t = (Time.time - _moveMoveStartTime) / _moonApproachSpeed;
                float expT = 1 + Mathf.Pow(t - 1, _moonLerpExp);
                _moon.transform.position = Vector3.Lerp(_moonStartPosition, _moonTarget.position, expT);
                _moon.transform.localScale = Vector3.Lerp(_moonStartScale, _moonTarget.localScale, expT);

                if (t >= 1)
                {
                    _movingMoon = false;
                    _moon.GetComponent<Levitate>().StartLevitateFromPosition();
                    _childBody.GetComponent<Levitate>().StopLevitatingAtNextBase();
                    _thundering = true;
                    ParticleSystem.MainModule m = _weather.GetThunderHitter().main;
                    m.startLifetime = new ParticleSystem.MinMaxCurve(_thunderLifespan);

                    _moonFadeBackStartTime = Time.time;
                }
            }
        }

        private void AxerWalk()
        {
            if (_axerWalkStartTime == 0) return;
            float t = (Time.time - _axerWalkStartTime) / 1.0f;
            if (t >= 1)
            {
                t = 1;
                _axerWalkStartTime = 0;
            }

            _axeSwingerTemp.transform.position = Vector3.Lerp(_axeSwingerStartPos, _axeSwingerEndPos.position, t);

            if (t >= 1)
            {
                _axeSwinger.transform.position = _axeSwingerTemp.position;
                _axeSwinger.transform.rotation = _axeSwingerTemp.rotation;
                _axeSwingerTemp.gameObject.SetActive(false);
                _axeSwinger.gameObject.SetActive(true);
                _axeSwinger.Play();
            }
        }

        private void CenterToChild()
        {
            if (_centerToChildStartTime == 0) return;
            float t = (Time.time - _centerToChildStartTime) / _splineSpeeds[1];
            if (t >= 1)
            {
                _centerToChildStartTime = 0;
                t = 1;
            }

            _ritualCenter.position = Vector3.Lerp(_startRitualCenter, _child.position, t);
        }

        private void MoveSpline()
        {
            if (_currentSpline == -1) return;

            Spline spline = _cineSplines[_currentSpline];

            float t = (Time.time - _splineStartTime) / _splineSpeeds[_currentSpline];

            if (t >= 1) t = 1;
                
            Unity.Mathematics.float3 position, tangent, up;
            if (spline.Evaluate(t, out position, out tangent, out up))
            {
                _cineCamera.position = position;
                _cineCamera.LookAt(_ritualCenter, Vector3.up);
            }

            if (t >= 1)
            {
                int splineFrom = _currentSpline;
                _currentSpline = -1;
                _splineStartTime = 0;

                if (splineFrom == 0)
                {
                    GameManager.GetMonoSystem<IUIMonoSystem>().Show<ChoiceView>();
                }
                if (splineFrom == 1) KillChild();
                if (splineFrom == 2) StartMoon();
            }
        }

        private void StartMoon()
        {
            _childBody.GetComponent<Levitate>().StartLevitateFromPosition();
            _moon.Find("Light").gameObject.SetActive(true);
            _movingMoon = true;
            _moonStartPosition = _moon.position;
            _moonStartScale = _moon.localScale;
            _moveMoveStartTime = Time.time;

            foreach (Transform torch in _torches)
            {
                torch.Find("Light").gameObject.SetActive(false);
                torch.Find("Particle System").gameObject.SetActive(false);
                MeshRenderer mr = torch.Find("Model").GetComponent<MeshRenderer>();
                mr.materials.ForEach(m => m.SetInt("Boolean_8BBF99CD", 1));
            }
        }

        private void KillChild()
        {
            _axerWalkStartTime = Time.time;
        }

        private void StartKillChild()
        {
            _currentSpline = 1;
            _splineStartTime = Time.time;
            
            BezierKnot k = _cineSplines[_currentSpline][0];
            k.Position = _cineCamera.position;
            k.Rotation = _cineCamera.rotation;
            _cineSplines[_currentSpline][0] = k;

            _centerToChildStartTime = Time.time;
        }

        public override void OnActEnd()
        {
        }

        public void SaveChild()
        {
            _saveChild = true;
            _playerMovingToCenter = true;
            HTJ21GameManager.Player.LookAt(_ritualCenter);
            HTJ21GameManager.Player.UncontrollableApproach = _playerApproachSpeed;
            HTJ21GameManager.Player.LockMovement = false;
        }

        public void SacrificeChild()
        {
            StartKillChild();
        }
    }
}
