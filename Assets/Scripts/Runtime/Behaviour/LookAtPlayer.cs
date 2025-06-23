using UnityEngine;

namespace HTJ21
{
    public class LookAtPlayer : MonoBehaviour
    {
        private void Update()
        {
            if (!HTJ21GameManager.CurrentControllable || HTJ21GameManager.IsPaused) return;

            transform.LookAt(HTJ21GameManager.CurrentControllable.transform, Vector3.up);
        }

    }
}
