using System;
using System.Collections.Generic;
using System.Linq;
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

        [Header("Seen Settings")]
        [SerializeField, ReadOnly] private List<CoverState> _hiddenStates;
        [SerializeField, ReadOnly] private bool _justRespawned = false;
        [SerializeField, ReadOnly] private float _timeSinceRespawned = 0f;
        [SerializeField] private float _gracePeriod = 0.1f;

        [SerializeField, ReadOnly] private Vector3 _cameraStartPos;
        [SerializeField, ReadOnly] private Quaternion _cameraStartRot;
        [SerializeField, ReadOnly] private Vector3 _playerStartPos;
        [SerializeField, ReadOnly] private Quaternion _playerStartRot;
        [SerializeField, ReadOnly] private Vector3 _headStartPos;
        [SerializeField, ReadOnly] private Quaternion _headStartRot;

        private float gravity = -9.81f;
        
        private float _defaultHeight;

        public bool LockMovement { get; set; }

        public bool LockMoving = false;

        public float UncontrollableApproach = 0;

        private float _smoothedXRot;
        private float _smoothedYRot;

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

            _timeSinceRespawned = 0f;
            _justRespawned = true;
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

        public void TurnOffLight()
        {
            _light.SetActive(false);   
        }

        public void TurnOnLight()
        {
            _light.SetActive(true);
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
                    rightSpeed, // * Time.deltaTime, 
                    0,
                    forwardSpeed), // * Time.deltaTime),
                ref _currentVel, 
                _settings.MovementSmoothing
            );
        }

        private void ProcessLook()
        {
            if (!_lookAt)
            {
                _smoothedXRot -= (_settings.InvertLookY ? -1 : 1) * _settings.Sensitivity.y * _inputHandler.RawLook.y;
                _smoothedXRot = Mathf.Clamp(_smoothedXRot, _settings.YLookLimit.x, _settings.YLookLimit.y);

                _smoothedYRot += (_settings.InvertLookX ? -1 : 1) * _settings.Sensitivity.x * _inputHandler.RawLook.x;
            }
            else
            {
                Vector3 dir = (_lookAt.position - _head.position).normalized;
                float targetXRot = -MathExt.Angle360To180(Vector3.SignedAngle(dir.SetY(0), dir, Vector3.Cross(dir, Vector3.up)));
                float targetYRot = MathExt.Angle360To180(Vector3.SignedAngle(Vector3.forward, dir.SetY(0), Vector3.up));

                _smoothedXRot = Mathf.Lerp(_smoothedXRot, targetXRot, Time.deltaTime * _lookAtSpeed);
                _smoothedYRot = Mathf.Lerp(_smoothedYRot, targetYRot, Time.deltaTime * _lookAtSpeed);
            }

            Quaternion headRotation = Quaternion.Euler(_smoothedXRot, 0f, 0f);
            _head.localRotation = headRotation;

            Quaternion playerRotation = Quaternion.Euler(0f, _smoothedYRot, 0f);
            transform.localRotation = playerRotation;
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

        public void SetHiddenState(CoverState state)
        {
            if (!_hiddenStates.Contains(state)) _hiddenStates.Add(state);
        }

        public void RemoveHiddenState(CoverState state)
        {
            _hiddenStates.Remove(state);
        }

        public bool IsInCover()
        {
            return _justRespawned || (_hiddenStates.Count > 0 && (_isCrouching || !_hiddenStates.Any(e => e.needToCrouch)));
        }

        public void ResetHead()
        {
            GetHead().transform.localPosition = _headStartPos;
            GetHead().transform.localRotation = _headStartRot;
        }

        public void ResetCamera()
        {
            GetCamera().transform.localPosition = _cameraStartPos;
            GetCamera().transform.localRotation = _cameraStartRot;
        }

        public void ResetPlayer() 
        {
            Teleport(_playerStartPos);
            transform.rotation = _playerStartRot;
        }

        private void Awake()
        {
            _inputHandler = GameManager.GetMonoSystem<IInputMonoSystem>();
            if (!_controller) _controller = GetComponent<CharacterController>();
            if (!_pickupManager) _pickupManager = GetComponent<PickupManager>();
            if (!_as) _as = GetComponent<AudioSource>();

            _hiddenStates = new List<CoverState>();
            _disableFlashlight = false;
            _light.SetActive(false);

            _cameraStartPos = GetCamera().transform.localPosition;
            _cameraStartRot = GetCamera().transform.localRotation;

            _playerStartPos = transform.position;
            _playerStartRot = transform.rotation;

            _headStartPos = GetHead().transform.localPosition;
            _headStartRot = GetHead().transform.localRotation;
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

            if (_justRespawned)
            {
                _timeSinceRespawned += Time.deltaTime;
                if (_timeSinceRespawned > _gracePeriod)
                {
                    _justRespawned = false;
                    _timeSinceRespawned = 0f;
                }
            }

            CheckIfInDoors();
            ProcessLook();
            ProcessMovement();
            ProcessGravity();

            if (_as && new Vector3(_movementSpeed.x, 0, _movementSpeed.z).magnitude > 0.01f) 
            {
                _as.clip = GameManager.GetMonoSystem<IDirectorMonoSystem>().IsCurrentActIndoors() ? _indoorsWalkClip : _outdoorsWalkClip;
                _as.pitch = GameManager.GetMonoSystem<IDirectorMonoSystem>().IsCurrentActIndoors() ? _indoorsPitch : _outdoorsPitch;
                if (!_as.isPlaying)
                {
                    _as.time = UnityEngine.Random.Range(0, _as.clip.length);
                    _as.Play();
                }
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

            _controller.Move(transform.TransformDirection(_movementSpeed *  Time.deltaTime));

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
