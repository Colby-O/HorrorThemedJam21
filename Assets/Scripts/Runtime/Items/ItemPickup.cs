using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class ItemPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject _outline;
        [SerializeField] private PickupableItem _type;

        public void AddOutline()
        {
            _outline.SetActive(true);
        }

        public void EndInteraction()
        {

        }

        public string GetHint()
        {
            return "Click 'E' to Pickup Flashlight";
        }

        public bool Interact(Interactor interactor)
        {
            if (HTJ21GameManager.PickupManager)
            {
                HTJ21GameManager.PickupManager.Pickup(_type);
                if (_type == PickupableItem.FlashLight) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(HTJ21GameManager.Preferences.PickupFlashlightDialogue);
            }
            Destroy(gameObject);
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
            _outline.SetActive(false);
        }

        private void Awake()
        {
            RemoveOutline();
        }
    }
}
