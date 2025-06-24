using UnityEngine;

namespace HTJ21
{
    public class MoonController : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;

        public bool IsInSpotlight(Transform target)
        {
            if (_spotLight.type != LightType.Spot) return false;

            float dst = Vector3.Distance(target.position, _spotLight.transform.position);
            if (dst > _spotLight.range) return false;

            float angle = Vector3.Angle(_spotLight.transform.forward, (target.position - _spotLight.transform.position).normalized);

            if (angle > _spotLight.spotAngle * 0.5f) return false;

            if (Physics.Raycast(_spotLight.transform.position, (target.position - _spotLight.transform.position).normalized, out RaycastHit hit, dst))
            {
                if (hit.transform != target)
                {
                    return false;
                }
            }

            return true;
        }

        private void Awake()
        {
            
        }

        private void Update()
        {

        }
    }
}
