using System.Collections.Generic;
using System.Linq;
using PlazmaGames.Core;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace HTJ21
{
    [System.Serializable]
    public class TorqueCurvePoint
    {
        public float rpm, torque;
    }

    [System.Serializable]
    public class CarInfo
    {
        public float airResistance;
        public float engineDynamicFriction;
        
        public float maxRpm;

        public float suspensionLength;
        public float suspensionStrength;
        public float suspensionDamping;

        public float wheelRadius;
        public float wheelStaticFriction;
        public float wheelKineticFriction;
        public float frontWheelTractionSpeed;
        public float rearWheelTractionSpeed;
        public float frontWheelSlideSpeed;
        public float rearWheelSlideSpeed;

        public float brakeForce;

        public float[] gearRatios;
        public float finalDriveRatio;

        public float weightDistribution;
        
        public TorqueCurvePoint[] torqueCurve;
        public float peakTorque;

        public float SampleTorqueCurve(float rpm)
        {
            for (int i = 1; i < torqueCurve.Length; i++)
            {
                if (rpm < torqueCurve[i].rpm)
                {
                    return MathExt.Remap(
                        rpm,
                        torqueCurve[i - 1].rpm, torqueCurve[i].rpm,
                        torqueCurve[i - 1].torque, torqueCurve[i].torque
                    ) * peakTorque;
                }
            }

            return 0;
        }

        public float GearRatio(int gear)
        {
            return gearRatios[gear + 1] * finalDriveRatio;
        }

        public float InverseGearRatio(int gear)
        {
            float ratio = GearRatio(gear);
            if (ratio == 0) return 0;
            else return 1.0f / ratio;
        }

        public void NormalizeTorqueCurve()
        {
            float relativePeakTorque = torqueCurve.Max(p => p.torque);
            foreach (TorqueCurvePoint p in torqueCurve)
            {
                p.torque /= relativePeakTorque;
            }
        }
    }
    
    public class CarController : MonoBehaviour
    {
        [SerializeField] private PlayerSettings _settings;
        [SerializeField] private TutorialController _tutorial;

        [SerializeField] private CarInfo info;

        [SerializeField] private float _steeringAngle = 0;
        [SerializeField] private float _throttle = 0;
        [SerializeField] private float _brake = 0;
        [SerializeField] private int _gear = 1;
        
        [SerializeField] private float _rpm = 0;
        [SerializeField] private float _speed= 0;

        [SerializeField] private bool[] _wheelsSliding = new bool[4];

        private Dictionary<string, DrivingProfileSO> _drivingProfiles = new();

        public DrivingProfileSO DrivingProfile;

        [Header("Headlights")]
        [SerializeField] private Light _headLight;

        [Header("Siren")]
        [SerializeField] private Light _blueLight;
        [SerializeField] private Light _redLight;
        [SerializeField] private AudioSource _sirenAudio;

        [Header("Cameras")]
        [SerializeField] private Camera _carCamera;
        [SerializeField] private MirrorCamera _leftMirrorCam;
        [SerializeField] private MirrorCamera _rightMirrorCam;
        [SerializeField] private MirrorCamera _backMirrorCam;


        private PlayerController _player;
        private EngineSound _engineSound;
        private IInputMonoSystem _inputHandler;
        private Rigidbody _rig;
        private Transform _steeringWheel;
        private Transform[] _wheels = new Transform[4];
        private Transform _camera;
        private Transform _cameraTarget;
        private Vector3 _cameraVelocity;
        private Transform _doorLocation;

        private bool _wasEnteredThisFrame = false;
        private bool _isDisabled = false;

        private Vector3 _lastVelocity;

        public TutorialController GetTutorial() => _tutorial;

        public bool InCar() => _camera.gameObject.activeSelf;

        public Camera GetCamera() => _carCamera;

        private bool _isLocked = false;

        public bool IsLocked() => _isLocked;

        public void Lock()
        {
            _isLocked = true;
        }

        public void Unlock()
        {
            _isLocked = false;
        }

        public void ToggleHeadLight()
        {
            if (!InCar()) return;
            _headLight.gameObject.SetActive(!_headLight.gameObject.activeSelf);
        }

        public void EnableSiren()
        {
            _blueLight.gameObject.SetActive(true);
            _redLight.gameObject.SetActive(true);
            _sirenAudio.Play();
        }

        public void DisableSiren()
        {
            _blueLight.gameObject.SetActive(false);
            _redLight.gameObject.SetActive(false);
            _sirenAudio.Stop();
        }

        public void DisableMirrors()
        {
            _backMirrorCam.Disable();
            _leftMirrorCam.Disable();
            _rightMirrorCam.Disable();
        }

        public void EnableMirrors()
        {
            _backMirrorCam.Enable();
            _leftMirrorCam.Enable();
            _rightMirrorCam.Enable();
        }

        public void SetDisableState(bool state)
        {
            _isDisabled = state;
            if (!state) _brake = 0;
        }

        private void LoadProfile(string name)
        {
            _drivingProfiles.Add(name, Resources.Load<DrivingProfileSO>($"DrivingProfiles/{name}"));
        }

        private void Awake()
        {
            LoadProfile("Normal");
            LoadProfile("Emergency");
            LoadProfile("Debug");
            _player = GameObject.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None)[0];
            _inputHandler = GameManager.GetMonoSystem<IInputMonoSystem>();
            _doorLocation = transform.Find("DoorLocation");
            _camera = transform.Find("Camera");
            _cameraTarget = transform.Find("CameraTarget");
            info.NormalizeTorqueCurve();
            _rig = GetComponent<Rigidbody>();
            _steeringWheel = transform.Find("Model").Find("SteeringWheel");
            Transform wheels = transform.Find("Wheels");
            for (int i = 0; i < wheels.childCount; i++) _wheels[i] = wheels.GetChild(i);
            _engineSound = GetComponent<EngineSound>();
            _inputHandler.LightCallback.AddListener(ToggleHeadLight);
            DisableSiren();
            Lock();
            _headLight.gameObject.SetActive(false);
        }

        private void ProcessLook()
        {
            Vector3 headRotation = _camera.localEulerAngles;
            headRotation.x -= (_settings.InvertLookY ? -1 : 1) * _settings.Sensitivity.y * _inputHandler.RawLook.y * Time.deltaTime;
            headRotation.x = Mathf.Clamp(Mathf.Repeat(headRotation.x + 180, 360) - 180, _settings.CarYLookLimit.x, _settings.CarYLookLimit.y);
            headRotation.y += (_settings.InvertLookX ? -1 : 1) * _settings.Sensitivity.x * _inputHandler.RawLook.x * Time.deltaTime;
            headRotation.y = Mathf.Clamp(Mathf.Repeat(headRotation.y + 180, 360) - 180, _settings.CarXLookLimit.x, _settings.CarXLookLimit.y);

            _camera.localRotation = Quaternion.Euler(headRotation);

            Vector3 cameraVelocity = _rig.GetPointVelocity(_cameraTarget.position);
            Vector3 acceleration = cameraVelocity - _cameraVelocity;
            _camera.position = Vector3.Lerp(_camera.position, _cameraTarget.position - acceleration * _settings.HeadBobbleMagnitude, Time.deltaTime * _settings.HeadBobbleSpeed);
            _cameraVelocity = cameraVelocity;
        }

        public void EnterCar()
        {
            if (_isLocked) return;
            _wasEnteredThisFrame = true;
            EnableMirrors();
            _camera.gameObject.SetActive(true);
        }
        
        private void ExitCar()
        {
            Debug.Log("EXIT CAR");
            if (_isLocked || _wasEnteredThisFrame || !InCar()) return;
            DisableMirrors();
            _camera.gameObject.SetActive(false);
            _player.EnterAt(_doorLocation.position);
        }

        private void Update()
        {
            if (!InCar() || HTJ21GameManager.IsPaused) return;
            ProcessLook();
            if (_inputHandler.ReversePressed())
            {
                if (_gear == -1) _gear = 1;
                else if (_gear == 1) _gear = -1;
            }

            if (_inputHandler.InteractPressed()) ExitCar();
        }

        private void LateUpdate()
        {
            _wasEnteredThisFrame = false;
        }

        private void FixedUpdate()
        {
            _rpm = Rpm();
            _speed = Speed() * 3.6f;

            if (InCar() && !_isDisabled && !HTJ21GameManager.IsPaused)
            {
                _throttle = Mathf.Max(0, _inputHandler.RawMovement.y) * DrivingProfile.MaxThrottle;
                _brake = Mathf.Max(0, -_inputHandler.RawMovement.y);
                _steeringAngle = Mathf.Lerp(_steeringAngle, _inputHandler.RawMovement.x * MaxTurnAngle(), Time.deltaTime * _settings.WheelTurnSpeed);
            }
            else
            {
                _throttle = 0;
                _brake = 1;
            }

            _wheels[0].localRotation = Quaternion.Euler(0, _steeringAngle, 90);
            _wheels[1].localRotation = Quaternion.Euler(0, _steeringAngle, 90);

            Vector3 swRot = _steeringWheel.localRotation.eulerAngles;
            swRot.z = 90 + _steeringAngle * _settings.SteeringWheelAngleScale;
            _steeringWheel.localRotation = Quaternion.Euler(swRot);

            _engineSound.SetRpmAndThrottle(_rpm, _throttle);

            Simulate();
        }

        private float MaxTurnAngle()
        {
            return Mathf.Lerp(_settings.MaxTurnAngle, _settings.MaxSpeedTurnAngle, Speed() / (_settings.MaxSpeedTurnAngleSpeed / 3.6f));
        }

        private void Simulate()
        {
            if (HTJ21GameManager.IsPaused)
            {
                if (!_rig.isKinematic) _lastVelocity = _rig.linearVelocity;
                _rig.isKinematic = true;
                return;
            }
            if (_brake > 0.7 && Mathf.Abs(Speed()) < _settings.ParkingSpeed)
            {
                if (!_rig.isKinematic) _lastVelocity = _rig.linearVelocity;
                _rig.isKinematic = true;
                return;
            }

            bool wasKinematic = _rig.isKinematic;
            _rig.isKinematic = false;
            if (wasKinematic) _rig.linearVelocity = _lastVelocity;

            if (DrivingProfile.MaxSpeed > 0 && Speed() * 3.6 > DrivingProfile.MaxSpeed) _throttle = 0;
            if (DrivingProfile.Automatic)
            {
                if (_rpm >= DrivingProfile.AutomaticUpShiftPoint && _gear >= 1) _gear = Mathf.Min(6, _gear + 1);
                if (_rpm <= DrivingProfile.AutomaticDownShiftPoint && _gear > 1) _gear = Mathf.Max(1, _gear - 1);
                _rpm = Rpm();
            }
            _rig.AddForce(-transform.forward * (info.airResistance * MathExt.Square(Vector3.Dot(transform.forward, _rig.linearVelocity))));
            for (int wheel = 0; wheel < 4; wheel++)
            {
                Vector3 wheelPosition = _wheels[wheel].position;
                Vector3 wheelUp = transform.TransformDirection(Vector3.up);
                Vector3 wheelForward = transform.TransformDirection(Vector3.forward);
                Vector3 wheelRight = transform.TransformDirection(Vector3.right);
                if (wheel < 2)
                {
                    wheelForward = Quaternion.AngleAxis(_steeringAngle, wheelUp) * wheelForward;
                    wheelRight = Quaternion.AngleAxis(_steeringAngle, wheelUp) * wheelRight;
                }

                Vector3 wheelVelocity = _rig.GetPointVelocity(wheelPosition);

                float slidingSpeed = Vector3.Dot(wheelRight, wheelVelocity);
                float absSlidingSpeed = Mathf.Abs(slidingSpeed);

                float give = 0.4f;
                if (Physics.Raycast(
                        wheelPosition + wheelUp * give,
                        -wheelUp,
                        out RaycastHit hit,
                        info.wheelRadius + give))
                {
                    Vector3 force = Vector3.zero;
                    float suspensionPosition = info.suspensionLength - (hit.distance - give);
                    Vector3 suspensionForce =
                        wheelUp * (
                            (suspensionPosition / info.suspensionLength * info.suspensionStrength) +
                            (Vector3.Dot(-wheelUp, wheelVelocity) * info.suspensionDamping));
                    force += suspensionForce;

                    float wheelTractionSpeed = wheel < 2 ? info.frontWheelTractionSpeed : info.rearWheelTractionSpeed;
                    float wheelSlideSpeed = wheel < 2 ? info.frontWheelSlideSpeed : info.rearWheelSlideSpeed;

                    if (absSlidingSpeed < wheelTractionSpeed)
                    {
                        _wheelsSliding[wheel] = false;
                    }

                    if (absSlidingSpeed > wheelSlideSpeed)
                    {
                        _wheelsSliding[wheel] = true;
                    }

                    float friction = _wheelsSliding[wheel] ? info.wheelKineticFriction : info.wheelStaticFriction;
                    Vector3 frictionForce = wheelRight * (-slidingSpeed * friction * Vector3.Dot(wheelUp, suspensionForce));
                    force += frictionForce;

                    force += wheelForward * (_throttle * DrivingProfile.TorqueScale * info.SampleTorqueCurve(_rpm) * info.GearRatio(_gear) / 2.0f);
                    force += -wheelForward * (MathExt.Sign(Vector3.Dot(wheelForward, wheelVelocity)) * _brake * info.brakeForce);
                    force += -wheelForward * (info.engineDynamicFriction * _rpm * (1.0f - _throttle) * info.GearRatio(_gear));

                    _rig.AddForceAtPosition(force, wheelPosition);
                }
            }
        }

        private float Speed()
        {
            return Vector3.Dot(transform.forward, _rig.linearVelocity);
        }
        
        private float Rpm()
        {
            return Mathf.Clamp(
                Speed() / 0.3f * 60.0f / (2.0f * Mathf.PI) * info.GearRatio(_gear) * _settings.RpmScale,
                0,
                info.maxRpm);
        }

        public bool SetDrivingProfile(string name)
        {
            if (_drivingProfiles.TryGetValue(name, out DrivingProfileSO profile))
            {
                DrivingProfile = profile;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
