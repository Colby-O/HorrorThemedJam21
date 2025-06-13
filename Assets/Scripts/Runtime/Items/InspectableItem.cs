using Unity.VisualScripting;
using UnityEngine;

namespace HTJ21
{
    public enum InspectType
    {
        Goto,
        ComeTo,
        Moveable
    }

    public class InspectableItem : MonoBehaviour, IInteractable
    {
        [SerializeField] InspectType _inspectType;
        [SerializeField] Transform _offset;

        public void AddOutline()
        {

        }

        public void EndInteraction()
        {

        }

        public string GetHint()
        {

            return $"Click 'E' to {((_inspectType == InspectType.Moveable) ? "pickup" : "inspect")}";
        }

        public bool Interact(Interactor interactor)
        {
            if (HTJ21GameManager.Inspector) HTJ21GameManager.Inspector.StartInspect(transform, _inspectType, _offset);
            return true;
        }

        public bool IsInteractable()
        {
            return true;
        }

        public void OnPickup(Interactor interactor)
        {

        }

        public void RemoveOutline()
        {

        }
    }
}
