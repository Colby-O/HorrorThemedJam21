using PlazmaGames.Core;
using PlazmaGames.UI;
using PlazmaGames.UI.Views;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace HTJ21
{
    [RequireComponent(typeof(PlayerInput))]
    public class InputMonoSystem : MonoBehaviour, IInputMonoSystem
    {
        private PlayerInput _input;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _interactAction;
        private InputAction _rAction;
        private InputAction _lightAction;
        private InputAction _skipAction;
        private InputAction _crouchAction;

        public Vector2 RawMovement { get; private set; }
        public Vector2 RawLook { get; private set; }
        public bool InteractPressed() => _interactAction.WasPerformedThisFrame();
        public bool LightPressed() => _lightAction.WasPerformedThisFrame();
        public bool Crouched() => _crouchAction.IsPressed();
        public bool JustCrouched() => _crouchAction.WasPressedThisFrame();
        public bool JustUncrouched() => _crouchAction.WasReleasedThisFrame();

        public UnityEvent InteractionCallback { get; private set; }
        public UnityEvent SkipCallback { get; private set; }
        public UnityEvent LightCallback { get; private set; }
        public UnityEvent RCallback { get; private set; }

        private void HandleMoveAction(InputAction.CallbackContext e)
        {
            RawMovement = e.ReadValue<Vector2>();
        }

        private void HandleLookAction(InputAction.CallbackContext e)
        {
            RawLook = e.ReadValue<Vector2>();
        }

        private void HandleInteractAction(InputAction.CallbackContext e)
        {
            if (HTJ21GameManager.IsPaused) return;
            InteractionCallback.Invoke();
        }

        private void HandleLightAction(InputAction.CallbackContext e)
        {
            if (HTJ21GameManager.IsPaused) return;
            LightCallback.Invoke();
        }

        private void HandleSkipAction(InputAction.CallbackContext e)
        {
            if (HTJ21GameManager.IsPaused) return;
            SkipCallback.Invoke();
        }

        private void HandleRAction(InputAction.CallbackContext e)
        {
            if (HTJ21GameManager.IsPaused) return;
            RCallback.Invoke();
        }

        private void Awake()
        {
            if (_input == null) _input = GetComponent<PlayerInput>();
            InteractionCallback = new UnityEvent();
            SkipCallback = new UnityEvent();
            LightCallback = new UnityEvent();
            RCallback = new UnityEvent();

            _moveAction = _input.actions["Move"];
            _lookAction = _input.actions["Look"];
            _lightAction = _input.actions["Light"];
            _interactAction = _input.actions["Interact"];
            _skipAction = _input.actions["Skip"];
            _rAction = _input.actions["RAction"];
            _crouchAction = _input.actions["Crouch"];

            _interactAction.performed += HandleInteractAction;
            _skipAction.performed += HandleSkipAction;
            _moveAction.performed += HandleMoveAction;
            _lookAction.performed += HandleLookAction;
            _lightAction.performed += HandleLightAction;
            _rAction.performed += HandleRAction;
            _crouchAction.performed += HandleRAction;
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame && !HTJ21GameManager.IsPaused && HTJ21GameManager.Inspector && HTJ21GameManager.Inspector.IsInspecting())
            {
                HTJ21GameManager.Inspector.EndInspect();
            }
            else if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
            {
                if (
                    !GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<PausedView>() && 
                    !GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<SettingsView>() &&
                    !GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<MainMenuView>() &&
                    !GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<DeveloperConsoleView>()
                ) GameManager.GetMonoSystem<IUIMonoSystem>().Show<PausedView>();
                else if (
                    !GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<MainMenuView>() &&
                    !GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<DeveloperConsoleView>()
                ) GameManager.GetMonoSystem<IUIMonoSystem>().GetView<PausedView>().Resume();
            }

            if (Keyboard.current.backquoteKey.wasPressedThisFrame && Keyboard.current.shiftKey.wasPressedThisFrame) 
            {
                if (!GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<DeveloperConsoleView>())
                {
                    HTJ21GameManager.IsPaused = true;
                    GameManager.GetMonoSystem<IUIMonoSystem>().Show<DeveloperConsoleView>();
                }
                else
                {
                    GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
                    HTJ21GameManager.IsPaused = GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<PausedView>() || GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<SettingsView>();
                }
            }
        }

        private void OnDestroy()
        {
            _interactAction.performed -= HandleInteractAction;
            _skipAction.performed -=HandleSkipAction;
            _moveAction.performed -= HandleMoveAction;
            _lookAction.performed -= HandleLookAction;
        }
    }
}
