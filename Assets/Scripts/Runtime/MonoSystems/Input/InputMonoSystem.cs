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
        private InputAction _reverseAction;
        private InputAction _interactAction;
        private InputAction _lightAction;
        private InputAction _skipAction;

        public Vector2 RawMovement { get; private set; }
        public Vector2 RawLook { get; private set; }
        public bool ReversePressed() => _reverseAction.WasPerformedThisFrame();
        public bool InteractPressed() => _interactAction.WasPerformedThisFrame();
        public bool LightPressed() => _lightAction.WasPerformedThisFrame();

        public UnityEvent InteractionCallback { get; private set; }
        public UnityEvent SkipCallback { get; private set; }
        public UnityEvent LightCallback { get; private set; }

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

        private void Awake()
        {
            if (_input == null) _input = GetComponent<PlayerInput>();
            InteractionCallback = new UnityEvent();
            SkipCallback = new UnityEvent();
            LightCallback = new UnityEvent();

            _moveAction = _input.actions["Move"];
            _lookAction = _input.actions["Look"];
            _reverseAction = _input.actions["Reverse"];
            _lightAction = _input.actions["Light"];
            _interactAction = _input.actions["Interact"];
            _skipAction = _input.actions["Skip"];

            _interactAction.performed += HandleInteractAction;
            _skipAction.performed += HandleSkipAction;
            _moveAction.performed += HandleMoveAction;
            _lookAction.performed += HandleLookAction;
            _lightAction.performed += HandleLightAction;
        }

        private void Update()
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
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

            if (Keyboard.current.backquoteKey.wasPressedThisFrame) 
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
