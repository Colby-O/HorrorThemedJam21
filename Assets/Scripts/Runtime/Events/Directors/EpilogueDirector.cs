using System.Collections.Generic;
using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;

namespace HTJ21
{
    public class EpilogueDirector : Director
    {
        [SerializeField] private List<GameObject> _items;

        [SerializeField] private Transform _playerStartLocation;
        [SerializeField] private Transform _carStartLocation;
        
        [SerializeField] private MeshRenderer _cultCover;
        [SerializeField] private Transform _ritualCenter;
        [SerializeField] private Transform _child;
        [SerializeField] private CultAnimator _axeSwinger;
        [SerializeField] private Transform _axeSwingerTemp;
        [SerializeField] private Transform _axeSwingerEndPos;
        
        [SerializeField] private Transform _nathanHead;
        [SerializeField] private Transform _nathanRealHead;
        [SerializeField] private Vector3 _headChopForce = Vector3.right;
        
        [SerializeField] private float _revealCultFadeTime = 2.0f;

        [SerializeField] private List<float> _splineSpeeds = new();
        [SerializeField] private Transform _cineCamera;
        [SerializeField] private SplineContainer _cineSplines;
        
        private float _revealCultStartTime = 0;

        private bool _atBloodTrail = false;
        private bool _turnedOffCarLights = false;
        
        private int _currentSpline = -1;
        private float _splineStartTime = 0;
        private float _centerToChildStartTime = 0;
        private Vector3 _startRitualCenter;

        private float _axerWalkStartTime = 0;
        private Vector3 _axeSwingerStartPos;
        private Vector3 _nathanHeadStartPosition;

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
            _nathanHeadStartPosition = _nathanRealHead.localPosition;
            _axeSwinger.OnHalfFinish.AddListener(ChopOffHead);
            _axeSwingerStartPos = _axeSwingerTemp.position;
            _startRitualCenter = _ritualCenter.position;
            _cineCamera.gameObject.SetActive(false);
            GameManager.AddEventListener<Events.RevealCult>(Events.NewRevealCult((from, data) =>
            {
                _revealCultStartTime = Time.time;
            }));
            GameManager.AddEventListener<Events.AtBloodTrail>(Events.NewAtBloodTrail((from, data) =>
            {
                _atBloodTrail = true;
            }));
        }

        private void ChopOffHead()
        {
            _nathanHead.gameObject.SetActive(true);
            _nathanHead.GetComponent<Rigidbody>().AddForce(_headChopForce, ForceMode.Impulse);
            _nathanRealHead.localPosition = _nathanRealHead.localPosition.AddY(-10);
        }

        public override void OnActStart()
        {
            _nathanRealHead.localPosition = _nathanHeadStartPosition;
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
            _cineSplines[0][0] = k;
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
                StartCinema();
            }

            Debug.Log(t);
            _cultCover.material.color = _cultCover.material.color.SetA(1.0f - t);
        }

        public override void OnActUpdate()
        {
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

                if (splineFrom == 0) StartKillChild();
                if (splineFrom == 1) KillChild();
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
    }
}
