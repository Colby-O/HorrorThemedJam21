using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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

        [SerializeField, ReadOnly] List<IInteractable> _lastListOfPossibleInteractable;

        private string _hint;

        private void StartInteraction(IInteractable interactable)
        {
            interactable.Interact(this);
        }

        private void CheckForInteractionInteract()
        {
            if (HTJ21GameManager.Inspector.IsInspecting()) return;

            if
            (
                Physics.Raycast(_head.position, (_interactionPoint.position - _head.position).normalized, out RaycastHit hit, _interactionRadius, _interactionLayer) ||
                Physics.SphereCast(_head.position, _spehreCastRadius, (_interactionPoint.position - _head.position).normalized, out hit, _interactionRadius, _interactionLayer)
            )
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null && interactable.IsInteractable()) StartInteraction(interactable);
            }
        }

        private void CheckForPossibleInteractionInteract()
        {
            if (HTJ21GameManager.Inspector.IsInspecting()) return;

            List<IInteractable> possibleInteractable = new List<IInteractable>();

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
                    possibleInteractable.Add(interactable);
                    if (!_lastListOfPossibleInteractable.Contains(interactable)) interactable.AddOutline();
                    if (interactable.GetHint() != string.Empty) GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SetHint(interactable.GetHint());
                }
            }

            foreach (IInteractable interactable in _lastListOfPossibleInteractable)
            {
                if (!possibleInteractable.Contains(interactable))
                {
                    if (interactable != null) interactable.RemoveOutline();
                }
            }

            _lastListOfPossibleInteractable = possibleInteractable;
        }

        private void Start()
        {
            _input = GameManager.GetMonoSystem<IInputMonoSystem>();
            _lastListOfPossibleInteractable = new List<IInteractable>();
            _input.InteractionCallback.AddListener(CheckForInteractionInteract);
        }

        private void OnDisable()
        {
            foreach (IInteractable interactable in _lastListOfPossibleInteractable)
            {
                interactable.RemoveOutline();
            }
            _lastListOfPossibleInteractable.Clear();
        }

        private void LateUpdate()
        {
            CheckForPossibleInteractionInteract();
        }
    }
}
