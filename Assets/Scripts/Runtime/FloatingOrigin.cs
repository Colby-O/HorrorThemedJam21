using UnityEngine;

namespace HTJ21
{
    public class FloatingOrigin : MonoBehaviour
    {
        public float threshold = 1000f;

        void Update()
        {
            if (!HTJ21GameManager.CurrentControllable) return;

            if (HTJ21GameManager.CurrentControllable.transform.position.magnitude > threshold)
            {
                Vector3 offset = HTJ21GameManager.CurrentControllable.transform.position;
                foreach (GameObject go in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
                {
                    if (go.transform.parent == null && go != HTJ21GameManager.CurrentControllable.gameObject)
                    {
                        go.transform.position -= offset;
                    }
                }
                HTJ21GameManager.CurrentControllable.transform.position = Vector3.zero;
            }
        }
    }
}
