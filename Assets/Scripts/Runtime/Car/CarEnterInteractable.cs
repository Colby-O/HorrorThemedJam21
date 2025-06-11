using UnityEngine;

namespace HTJ21
{
    public class CarEnterInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject _outline;
        public void AddOutline()
        {
            _outline.SetActive(true);
        }

        public void EndInteraction()
        {

        }

        public string GetHint()
        {
            return "Click 'E' To Enter";
        }

        public bool Interact(Interactor interactor)
        {
            if (HTJ21GameManager.Player.IsInCar()) return true;
            HTJ21GameManager.Player.EnterCar();
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

        private void LateUpdate()
        {
            RemoveOutline();
        }
    }
}
