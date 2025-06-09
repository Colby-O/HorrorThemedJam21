using System.Diagnostics;
using PlazmaGames.Attribute;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HTJ21
{
    [RequireComponent(typeof(CharacterController), typeof(InputHandler))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterController _controller;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private Transform _head;
        [SerializeField] private PlayerSettings _settings;

        [SerializeField, ReadOnly] private Vector3 _movementSpeed;
        [SerializeField, ReadOnly] private Vector3 _currentVel;
        [SerializeField, ReadOnly] private Vector3 _bodyRotation;
        [SerializeField, ReadOnly] private Vector3 _headRotation;
        [SerializeField, ReadOnly] private float _velY;

        private float gravity = -9.81f;

        public bool LockMovement { get; set; }

        public void EnterAt(Vector3 doorLocationPosition)
        {
            transform.position = doorLocationPosition;
            _head.gameObject.SetActive(true);
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

        private void Awake()
        {
            if (_inputHandler == null) _inputHandler = GetComponent<InputHandler>();
            if (_controller == null) _controller = GetComponent<CharacterController>();

            _bodyRotation = transform.localRotation.eulerAngles;
            _headRotation = _head.localRotation.eulerAngles;
        }

        private void Update()
        {
            if (LockMovement) return;

            ProcessLook();
            ProcessMovement();
            ProcessGravity();
            _controller.Move(transform.TransformDirection(_movementSpeed));
        }
    }
}
