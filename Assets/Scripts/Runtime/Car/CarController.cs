using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public float wheelTractionSpeed;
        public float wheelSlideSpeed;

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

    [System.Serializable]
    public class DrivingProfile
    {
        public float wheelTurnSpeed = 4;
        public float steeringWheelAngleScale = 1.0f;
        public float maxTurnAngle = 35;
        public float maxSpeedTurnAngle = 10;
        public float maxSpeedTurnAngleSpeed = 100;
        public float maxThrottle = 1;
        public float maxSpeed = 0;
        public bool automatic = true;
        public float automaticUpShiftPoint;
        public float automaticDownShiftPoint;
    }

    public class CarController : MonoBehaviour
    {
        private EngineSound _engineSound;
        
        [SerializeField] private CarInfo info;

        [SerializeField] private float _steeringAngle = 0;
        [SerializeField] private float _throttle = 0;
        [SerializeField] private float _brake = 0;
        [SerializeField] private int _gear = 1;
        
        [SerializeField] private float _rpm = 0;
        [SerializeField] private float _speed= 0;

        [SerializeField] private bool[] _wheelsSliding = new bool[4];

        private Rigidbody _rig;

        private Transform _steeringWheel;
        private Transform[] _wheels = new Transform[4];
        [SerializeField] private float _rpmScale = 10;
        [SerializeField] private DrivingProfile _drivingProfile;


        private void Awake()
        {
            info.NormalizeTorqueCurve();
            _rig = GetComponent<Rigidbody>();
            _steeringWheel = transform.Find("Model").Find("SteeringWheel");
            Transform wheels = transform.Find("Wheels");
            for (int i = 0; i < wheels.childCount; i++) _wheels[i] = wheels.GetChild(i);
            _engineSound = GetComponent<EngineSound>();
        }

        private void Update()
        {
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
            {
                if (_drivingProfile.automatic)
                {
                    if (_gear == -1) _gear = 1;
                    else _gear += 1;
                }
                else
                {
                    _gear += 1;
                }
            }

            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
            {
                if (_drivingProfile.automatic)
                {
                    if (_gear == 1) _gear = -1;
                    else _gear -= 1;
                }
                else
                {
                    _gear -= 1;
                }
            }
            _gear = Mathf.Clamp(_gear, -1, 6);
        }

        private void FixedUpdate()
        {
            _rpm = Rpm();
            _speed = Speed() * 3.6f;
            
            if (Keyboard.current.wKey.IsPressed()) _throttle = _drivingProfile.maxThrottle;
            if (!Keyboard.current.wKey.IsPressed()) _throttle = 0;
            if (Keyboard.current.sKey.IsPressed()) _brake = 1f;
            if (!Keyboard.current.sKey.IsPressed()) _brake = 0;

            if (Keyboard.current.aKey.IsPressed())
            {
                _steeringAngle = Mathf.Lerp(_steeringAngle, -MaxTurnAngle(), Time.deltaTime * _drivingProfile.wheelTurnSpeed);
            }
            else if (Keyboard.current.dKey.IsPressed())
            {
                _steeringAngle = Mathf.Lerp(_steeringAngle, MaxTurnAngle(), Time.deltaTime * _drivingProfile.wheelTurnSpeed);
            }
            else
            {
                _steeringAngle = Mathf.Lerp(_steeringAngle, 0, Time.deltaTime * _drivingProfile.wheelTurnSpeed);
            }

            _wheels[0].localRotation = Quaternion.Euler(0, _steeringAngle, 90);
            _wheels[1].localRotation = Quaternion.Euler(0, _steeringAngle, 90);
            Vector3 swRot = _steeringWheel.localRotation.eulerAngles;
            swRot.z = 90 + _steeringAngle * _drivingProfile.steeringWheelAngleScale;
            _steeringWheel.localRotation = Quaternion.Euler(swRot);

            
            _engineSound.SetRpmAndThrottle(_rpm, _throttle);

            Simulate();
        }

        private float MaxTurnAngle()
        {
            return Mathf.Lerp(_drivingProfile.maxTurnAngle, _drivingProfile.maxSpeedTurnAngle, Speed() / (_drivingProfile.maxSpeedTurnAngleSpeed / 3.6f));
        }

        private void Simulate()
        {
            if (_drivingProfile.maxSpeed > 0 && Speed() * 3.6 > _drivingProfile.maxSpeed) _throttle = 0;
            if (_drivingProfile.automatic)
            {
                if (_rpm >= _drivingProfile.automaticUpShiftPoint && _gear >= 1) _gear = Mathf.Min(6, _gear + 1);
                if (_rpm <= _drivingProfile.automaticDownShiftPoint && _gear > 1) _gear = Mathf.Max(1, _gear - 1);
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

                if (Physics.Raycast(
                        wheelPosition,
                        -wheelUp,
                        out RaycastHit hit,
                        info.wheelRadius))
                {
                    Debug.DrawRay(wheelPosition, -wheelUp * hit.distance, Color.yellow);
                    Vector3 force = Vector3.zero;
                    float suspensionPosition = info.suspensionLength - hit.distance;
                    Vector3 suspensionForce =
                        wheelUp * (
                            (suspensionPosition / info.suspensionLength * info.suspensionStrength) +
                            (Vector3.Dot(-wheelUp, wheelVelocity) * info.suspensionDamping));
                    force += suspensionForce;

                    if (absSlidingSpeed < info.wheelTractionSpeed)
                    {
                        _wheelsSliding[wheel] = false;
                    }

                    if (absSlidingSpeed > info.wheelSlideSpeed)
                    {
                        _wheelsSliding[wheel] = true;
                    }

                    float friction = _wheelsSliding[wheel] ? info.wheelKineticFriction : info.wheelStaticFriction;
                    Vector3 frictionForce = wheelRight * (-slidingSpeed * friction * Vector3.Dot(wheelUp, suspensionForce));
                    force += frictionForce;

                    if (wheel >= 2)
                    {
                        force += wheelForward * (_throttle * info.SampleTorqueCurve(_rpm) * info.GearRatio(_gear));
                        force += -wheelForward * (MathExt.Sign(Vector3.Dot(wheelForward, wheelVelocity)) * _brake * info.brakeForce);
                        force += -wheelForward * (info.engineDynamicFriction * _rpm * (1.0f - _throttle) * info.GearRatio(_gear));
                    }

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
                Speed() / 0.3f * 60.0f / (2.0f * Mathf.PI) * info.GearRatio(_gear) * _rpmScale,
                0,
                info.maxRpm);
        }
    }
}
