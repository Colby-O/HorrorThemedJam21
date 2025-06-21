using UnityEngine;

namespace HTJ21
{
    public class Crowbar : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Board"))
            {
                if (other.gameObject.TryGetComponent(out Rigidbody rig))
                {
                    rig.isKinematic = false;
                }
            }
        }
    }
}
