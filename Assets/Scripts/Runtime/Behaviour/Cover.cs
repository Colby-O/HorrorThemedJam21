using UnityEngine;

namespace HTJ21
{
    public class Cover : MonoBehaviour
    {
        [SerializeField] private bool _needToCourch = false;

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.SetHiddenState(_needToCourch);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.ClearHiddenState();
            }
        }
    }
}
