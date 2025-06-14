using UnityEngine;

namespace HTJ21
{
    public interface IInteractable 
    {
        public void AddOutline();

        public void RemoveOutline();

        public bool IsInteractable();

        public bool Interact(Interactor interactor);

        public void EndInteraction();

        public string GetHint();
    }
}
