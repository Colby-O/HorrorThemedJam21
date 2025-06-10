using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HTJ21
{
    public class Interactor : MonoBehaviour
    {
        [Header("References")]
        private IInputMonoSystem _input;
        [SerializeField] private Transform _head;

        [Header("Settings")]
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private float _interactionRadius = 0.1f;
        [SerializeField] private float _spehreCastRadius = 0.1f;

        private InputAction _interactAction;
        private InputAction _pickupAction;


        private string _hint;

        private void StartInteraction(IInteractable interactable)
        {
            interactable.Interact(this);
        }
        private void StartPickup(IInteractable interactable)
        {
            interactable.OnPickup(this);
        }

        private void CheckForInteractionInteract()
        {
            if
            (
                Physics.Raycast(_head.position, (_interactionPoint.position - _head.position).normalized, out RaycastHit hit, _interactionRadius, _interactionLayer) ||
                Physics.SphereCast(_head.position, _spehreCastRadius, (_interactionPoint.position - _head.position).normalized, out hit, _interactionRadius, _interactionLayer)
            )
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null) StartInteraction(interactable);
            }
        }

        private void CheckForInteractionPickup()
        {
            if
            (
                Physics.Raycast(_head.position, (_interactionPoint.position - _head.position).normalized, out RaycastHit hit, _interactionRadius, _interactionLayer) ||
                Physics.SphereCast(_head.position, _spehreCastRadius, (_interactionPoint.position - _head.position).normalized, out hit, _interactionRadius, _interactionLayer)
            )
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            }
        }

        private void CheckForPossibleInteractionInteract()
        {
            if
            (
                Physics.Raycast(_head.position, (_interactionPoint.position - _head.position).normalized, out RaycastHit hit, _interactionRadius, _interactionLayer) ||
                Physics.SphereCast(_head.position, _spehreCastRadius, (_interactionPoint.position - _head.position).normalized, out hit, _interactionRadius, _interactionLayer)
            )
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    if (!interactable.IsInteractable()) return;
                    interactable.AddOutline();
                }
            }
        }

        private void Start()
        {
            _input = GameManager.GetMonoSystem<IInputMonoSystem>();

            _input.InteractionCallback.AddListener(CheckForInteractionInteract);
        }

        private void LateUpdate()
        {
            CheckForPossibleInteractionInteract();
        }
    }
}
