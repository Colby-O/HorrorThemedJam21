using UnityEngine;
using UnityEngine.InputSystem;

namespace HTJ21
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerInput _input;

        private InputAction _moveAction;
        private InputAction _lookAction;

        public Vector2 RawMovement { get; private set; }
        public Vector2 RawLook { get; private set; }

        private void HandleMoveAction(InputAction.CallbackContext e)
        {
            RawMovement = e.ReadValue<Vector2>();
        }

        private void HandleLookAction(InputAction.CallbackContext e)
        {
            RawLook = e.ReadValue<Vector2>();
        }

        private void Awake()
        {
            if (_input == null) _input = GetComponent<PlayerInput>();

            _moveAction = _input.actions["Move"];
            _lookAction = _input.actions["Look"];

            _moveAction.performed += HandleMoveAction;
            _lookAction.performed += HandleLookAction;
        }

        private void OnDestroy()
        {
            _moveAction.performed -= HandleMoveAction;
            _lookAction.performed -= HandleLookAction;
        }
    }
}
