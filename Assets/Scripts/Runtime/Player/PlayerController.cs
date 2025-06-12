using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;

namespace HTJ21
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController _controller;
        private IInputMonoSystem _inputHandler;
        [SerializeField] private Transform _head;
        [SerializeField] private GameObject _light;
        [SerializeField] private PlayerSettings _settings;
        [SerializeField] private Camera _camera;

        [SerializeField, ReadOnly] private Vector3 _movementSpeed;
        [SerializeField, ReadOnly] private Vector3 _currentVel;
        [SerializeField, ReadOnly] private Vector3 _bodyRotation;
        [SerializeField, ReadOnly] private Vector3 _headRotation;
        [SerializeField, ReadOnly] private float _velY;

        private float gravity = -9.81f;

        public bool LockMovement { get; set; }

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
            MoveTo(position);
            _head.gameObject.SetActive(true);
        }

        public void EnterCar()
        {
            _head.gameObject.SetActive(false);
            HTJ21GameManager.Car.EnterCar();
        }

        private void ToggleLight()
        {
            _light.SetActive(!_light.activeSelf);
        }

        private void ProcessMovement()
        {
            float verticalSpeed = (_inputHandler.RawMovement.y == 1) ? _settings.WalkingForwardSpeed : _settings.WalkingBackwardSpeed;
            float horizontalSpeed = _settings.WalkingStrideSpeed;

            verticalSpeed *= _settings.Speed;
            horizontalSpeed *= _settings.Speed;

            _movementSpeed = Vector3.SmoothDamp(
                _movementSpeed, 
                new Vector3(-verticalSpeed * _inputHandler.RawMovement.y * Time.deltaTime, 0, horizontalSpeed * _inputHandler.RawMovement.x * Time.deltaTime), 
                ref _currentVel, 
                _settings.MovementSmoothing
            );
        }

        private void ProcessLook()
        {
            _headRotation.z -= (_settings.InvertLookY ? -1 : 1) * _settings.Sensitivity.y * _inputHandler.RawLook.y * Time.deltaTime;
            _headRotation.z = Mathf.Clamp(_headRotation.z, _settings.YLookLimit.x, _settings.YLookLimit.y);
            _bodyRotation.y += (_settings.InvertLookX ? -1 : 1) * _settings.Sensitivity.x * _inputHandler.RawLook.x * Time.deltaTime;

            _head.localRotation = Quaternion.Euler(_headRotation);
            transform.localRotation = Quaternion.Euler(_bodyRotation);
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
            if (_controller == null) _controller = GetComponent<CharacterController>();

            _bodyRotation = transform.localRotation.eulerAngles;
            _headRotation = _head.localRotation.eulerAngles;

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
