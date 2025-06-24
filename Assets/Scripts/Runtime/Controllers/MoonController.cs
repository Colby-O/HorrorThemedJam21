using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public class MoonController : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;
        [SerializeField] private Transform _raycastSource;
        [SerializeField] private float _visibilityThreshold = 0.5f;

        public UnityEvent OnPlayerHit;
        bool IsTargetVisibleInSpotlight(Transform spotlight, Light spotLightComponent, Renderer targetRenderer, Transform caster, float visibilityThreshold = 0.5f)
        {
            if (spotLightComponent.type != LightType.Spot)
                return false;

            Bounds bounds = targetRenderer.bounds;
            Vector3[] samplePoints = new Vector3[]
            {
                bounds.center,
                bounds.min,
                bounds.max,
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), // top-left-front
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), // top-right-front
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), // bottom-left-back
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), // bottom-right-back
            };

            int visibleCount = 0;

            foreach (var point in samplePoints)
            {
                Vector3 toPoint = point - spotlight.position;
                float distance = toPoint.magnitude;

                // 1. Check angle
                float angle = Vector3.Angle(spotlight.forward, toPoint);
                if (angle > spotLightComponent.spotAngle * 0.5f)
                    continue;

                // 2. Check distance
                if (distance > spotLightComponent.range)
                    continue;

                // 3. Raycast
                if (Physics.Raycast(spotlight.position, toPoint.normalized, out RaycastHit hit, distance))
                {
                    if (hit.transform == caster || hit.collider.transform.IsChildOf(caster.transform))
                    {
                        visibleCount++;
                    }
                }
                else
                {
                    // Nothing hit, assume visible
                    visibleCount++;
                }
            }

            float visibilityRatio = (float)visibleCount / samplePoints.Length;
            return visibilityRatio >= visibilityThreshold;
        }

        private void Awake()
        {
            OnPlayerHit = new UnityEvent();
        }

        private void Update()
        {
            if (HTJ21GameManager.Player && IsTargetVisibleInSpotlight(_spotLight.transform, _spotLight, HTJ21GameManager.Player.GetRenderer(), HTJ21GameManager.Player.transform, _visibilityThreshold))
            {
                OnPlayerHit.Invoke();
            }
        }
    }
}
