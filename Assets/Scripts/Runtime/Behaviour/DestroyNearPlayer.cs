using UnityEngine;

namespace HTJ21
{
    public class DestroyNearPlayer : MonoBehaviour
    {
        [SerializeField] private float _dst;

        private void Update()
        {
            if (!HTJ21GameManager.CurrentControllable || HTJ21GameManager.IsPaused) return;

            if (Vector3.Distance(HTJ21GameManager.CurrentControllable.transform.position, transform.position) < _dst)
            {
                Destroy(gameObject);
            }
        }
    }
}
