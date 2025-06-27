using System;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using Unity.VisualScripting;
using UnityEngine;

namespace HTJ21
{
    [RequireComponent(typeof(CharacterController), typeof(PickupManager))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController _controller;
        private IInputMonoSystem _inputHandler;
        [SerializeField] private Transform _head;
        [SerializeField] private GameObject _light;
        [SerializeField] private PlayerSettings _settings;
        [SerializeField] private Camera _camera;
        [SerializeField] private PickupManager _pickupManager;
        [SerializeField] private TutorialController _tutorial;
        [SerializeField] private Transform _center;
        [SerializeField] private Renderer _renderer;

        [Header("Audio")]
        [SerializeField] private AudioSource _as;
        [SerializeField] private AudioClip _indoorsWalkClip;
        [SerializeField] private float _indoorsPitch = 1.25f;
        [SerializeField] private float _outdoorsPitch = 1f;
        [SerializeField] private AudioClip _outdoorsWalkClip;

        [Header("Animation")]
        [SerializeField] private PlayerAnimationController _animator;

        [SerializeField, ReadOnly] private Vector3 _movementSpeed;
        [SerializeField, ReadOnly] private Vector3 _currentVel;
        [SerializeField, ReadOnly] private float _velY;
        
        [SerializeField] private Transform _lookAt = null;
        [SerializeField] private float _lookAtSpeed = 4;

        [SerializeField, ReadOnly] private bool _disableFlashlight = false;
        [SerializeField, ReadOnly] private bool _isCrouching;

        private float gravity = -9.81f;
        
        private float _defaultHeight;

        public bool LockMovement { get; set; }

        public bool LockMoving = false;

        public float UncontrollableApproach = 0;

        public void LookAt(Transform t) => _lookAt = t;

        public void StopLookAt() => _lookAt = null;

        public Renderer GetRenderer() => _renderer;

        public Transform GetCenter() => _center;

        public Transform GetHead() => _head;

        public TutorialController GetTutorial() => _tutorial;

        public Camera GetCamera() => _camera;

        public bool IsInCar() => !_head.gameObject.activeSelf;

        private void MoveTo(Vector3 position)
        {
            _controller.enabled = false;
            transform.position = position;
            _controller.enabled = true;
        }

        public void Teleport(Vector3 pos)
        {
            _controller.enabled = false;
            transform.position = pos;
            _controller.enabled = true;
        }

        public void EnterAt(Vector3 position)
        {
            gameObject.SetActive(true);
            MoveTo(position);
            _head.gameObject.SetActive(true);
        }

        public void EnterCar()
        {
            _head.gameObject.SetActive(false);
            if (HTJ21GameManager.Car) HTJ21GameManager.Car.EnterCar();
            gameObject.SetActive(false);
        }
        
        public void EnablePlayer()
        {
            gameObject.SetActive(true);
            _head.gameObject.SetActive(true);
        }

        public void DisablePlayer()
        {
            gameObject.SetActive(false);
            _head.gameObject.SetActive(false);
        }

        public void DisableFlashlight()
        {
            _disableFlashlight = true;
            _light.SetActive(false);
        }

        public void EnableFlashlight()
        {
            _disableFlashlight = false;
        }

        private void ToggleLight()
        {
            if (!_pickupManager.HasItem(PickupableItem.FlashLight) || _disableFlashlight) return;
            _light.SetActive(!_light.activeSelf);
        }

        private void ProcessMovement()
        {
            if (LockMoving)
            {
                _movementSpeed = Vector3.zero;
                return;
            }

            float dirSpeed = (_isCrouching ? _settings.CrouchSpeedMul : 1f) * ((_inputHandler.RawMovement.y == 1) ? _settings.WalkingForwardSpeed : _settings.WalkingBackwardSpeed);
            float forwardSpeed = _inputHandler.RawMovement.y * dirSpeed;
            float rightSpeed = _inputHandler.RawMovement.x * (_isCrouching ? _settings.CrouchSpeedMul : 1f) * _settings.WalkingStrideSpeed;

            if (UncontrollableApproach != 0)
            {
                if (forwardSpeed >= 0)
                {
                    forwardSpeed = UncontrollableApproach;
                }
                else
                {
                    forwardSpeed = -UncontrollableApproach / 3;
                }
            }
            else
            {
                forwardSpeed *= _settings.Speed;
            }
            
            rightSpeed *= _settings.Speed;

            _movementSpeed = Vector3.SmoothDamp(
                _movementSpeed, 
                new Vector3(
                    rightSpeed * Time.deltaTime, 
                    0,
                    forwardSpeed * Time.deltaTime),
                ref _currentVel, 
                _settings.MovementSmoothing
            );
        }

        private void ProcessLook()
        {
            float xRot = _head.localEulerAngles.x;
            float yRot = transform.localEulerAngles.y;
            if (!_lookAt)
            {
                xRot -= (_settings.InvertLookY ? -1 : 1) * _settings.Sensitivity.y * _inputHandler.RawLook.y * Time.deltaTime;
                xRot = Mathf.Clamp(MathExt.Angle360To180(xRot), _settings.YLookLimit.x, _settings.YLookLimit.y);
                yRot += (_settings.InvertLookX ? -1 : 1) * _settings.Sensitivity.x * _inputHandler.RawLook.x * Time.deltaTime;
            }
            else
            {
                Vector3 dir = Vector3.Normalize(_lookAt.position - _head.position);
                float targetXRot = -MathExt.Angle360To180(Vector3.SignedAngle(dir.SetY(0), dir, Vector3.Cross(dir, Vector3.up)));
                float targetYRot = MathExt.Angle360To180(Vector3.SignedAngle(Vector3.forward, dir.SetY(0), Vector3.up));
                xRot = targetXRot;
                yRot = targetYRot;
            }
            
            _head.localEulerAngles = _head.localEulerAngles.SetX(xRot);
            transform.localEulerAngles = transform.localEulerAngles.SetY(yRot);
        }

        private void ProcessGravity()
        {
            if (_controller.isGrounded && _velY < 0.0f) _velY = 0.0f;
            else _velY += _settings.GravityMultiplier * gravity * Time.deltaTime;

            _movementSpeed.y = _velY;
        }

        public bool CheckIfInDoors()
        {
            bool isIndoors = IsInCar() || GameManager.GetMonoSystem<IDirectorMonoSystem>().IsCurrentActIndoors();
            return isIndoors && HTJ21GameManager.HasStarted;
        }

        private void Awake()
        {
            _inputHandler = GameManager.GetMonoSystem<IInputMonoSystem>();
            if (!_controller) _controller = GetComponent<CharacterController>();
            if (!_pickupManager) _pickupManager = GetComponent<PickupManager>();
            if (!_as) _as = GetComponent<AudioSource>();

            _disableFlashlight = false;
            _light.SetActive(false);
        }

        private void Start()
        {
            _defaultHeight = _head.localPosition.y;
        }

        private void Update()
        {
            if (LockMovement || HTJ21GameManager.IsPaused)
            {
                if (_as && _as.isPlaying) _as.Stop();
                return;
            }

            CheckIfInDoors();
            ProcessLook();
            ProcessMovement();
            ProcessGravity();

            if (_as && new Vector3(_movementSpeed.x, 0, _movementSpeed.z).magnitude > 0.01f) 
            {
                _as.clip = GameManager.GetMonoSystem<IDirectorMonoSystem>().IsCurrentActIndoors() ? _indoorsWalkClip : _outdoorsWalkClip;
                _as.pitch = GameManager.GetMonoSystem<IDirectorMonoSystem>().IsCurrentActIndoors() ? _indoorsPitch : _outdoorsPitch;
                if (!_as.isPlaying) _as.Play();
                if (_animator) _animator.SetAnimationState(PlayerAnimationState.Walking);
            }
            else
            {
                if (_as) _as.Stop();
                if (_animator) _animator.SetAnimationState(PlayerAnimationState.Idle);
            }

            if (_inputHandler.JustCrouched())
            {
                _isCrouching = true;
                if (_animator) _animator.SetCrouchState(true);
                _head.localPosition = _head.localPosition.SetY(_settings.CrouchHeight);
                _controller.height = _settings.CrouchHeight + 0.3f;
                _controller.center = _controller.center.SetY(_controller.height / 2);
            }

            if (_inputHandler.JustUncrouched())
            {
                _isCrouching = false;
                if (_animator) _animator.SetCrouchState(false);
                _head.localPosition = _head.localPosition.SetY(_defaultHeight);
                _controller.height = _defaultHeight + 0.3f;
                _controller.center = _controller.center.SetY(_controller.height / 2);
            }

            _controller.Move(transform.TransformDirection(_movementSpeed));

            if (!IsInCar() && _inputHandler.LightPressed()) ToggleLight();
        }

        private void OnTriggerEnter(Collider hit)
        {
            if (hit.gameObject.CompareTag("SoccerBall"))
            {
                if (hit.gameObject.TryGetComponent(out Rigidbody rig))
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    rig.AddForce(dir * HTJ21GameManager.Preferences.SoccerBallHitForce, ForceMode.Impulse);
                }
            }
            else if (hit.gameObject.CompareTag("Hanging"))
            {
                if (hit.gameObject.TryGetComponent(out Rigidbody rig))
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    rig.AddForce(dir * HTJ21GameManager.Preferences.HangingHitForce, ForceMode.Impulse);
                }
            }
        }
    }
}
