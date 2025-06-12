using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
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

        [SerializeField, ReadOnly] private Vector3 _movementSpeed;
        [SerializeField, ReadOnly] private Vector3 _currentVel;
        [SerializeField, ReadOnly] private float _velY;
        
        [SerializeField] private Transform _lookAt = null;
        [SerializeField] private float _lookAtSpeed = 4;

        private float gravity = -9.81f;

        public bool LockMovement { get; set; }

        public bool LockMoving = false;
        public float UncontrollableApproach = 0;

        public void LookAt(Transform t) => _lookAt = t;
        public void StopLookAt() => _lookAt = null;

        public Camera GetCamera() => _camera;

        public bool IsInCar() => !_head.gameObject.activeSelf;

        private void MoveTo(Vector3 position)
        {
            _controller.enabled = false;
            transform.position = position;
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
            HTJ21GameManager.Car.EnterCar();
            gameObject.SetActive(false);
        }

        private void ToggleLight()
        {
            if (!_pickupManager.HasItem(PickupableItem.FlashLight)) return;
            _light.SetActive(!_light.activeSelf);
        }

        private void ProcessMovement()
        {
            if (LockMoving)
            {
                _movementSpeed = Vector3.zero;
                return;
            }

            float dirSpeed = (_inputHandler.RawMovement.y == 1) ? _settings.WalkingForwardSpeed : _settings.WalkingBackwardSpeed;
            float forwardSpeed = _inputHandler.RawMovement.y * dirSpeed;
            float rightSpeed = _inputHandler.RawMovement.x * _settings.WalkingStrideSpeed;

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
            if (GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<MainMenuView>()) return false;
            bool isIndoors = IsInCar();
            return isIndoors;
        }

        private void Awake()
        {
            _inputHandler = GameManager.GetMonoSystem<IInputMonoSystem>();
            if (!_controller) _controller = GetComponent<CharacterController>();
            if (!_pickupManager) _pickupManager = GetComponent<PickupManager>();

            _light.SetActive(false);
            _head.gameObject.SetActive(false);
        }

        private void Start()
        {

        }

        private void Update()
        {
            if (LockMovement || HTJ21GameManager.IsPaused) return;
            CheckIfInDoors();
            ProcessLook();
            ProcessMovement();
            ProcessGravity();
            _controller.Move(transform.TransformDirection(_movementSpeed));

            if (!IsInCar() && _inputHandler.LightPressed()) ToggleLight();
        }
    }
}
