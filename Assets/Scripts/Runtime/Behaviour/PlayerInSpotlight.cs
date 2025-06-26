using PlazmaGames.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public class PlayerInSpotlight : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;
        [SerializeField] private float _visibilityThreshold = 0.5f;

        [SerializeField, ReadOnly] private bool _isEnabled = true; 

        public UnityEvent OnPlayerHit = new UnityEvent();


        public void Enable()
        {
            _isEnabled = true;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        private bool IsTargetVisibleInSpotlight(Transform spotlight, Light spotLightComponent, Renderer targetRenderer, Transform caster, float visibilityThreshold = 0.5f)
        {
            if (spotLightComponent.type != LightType.Spot)
                return false;

            Bounds bounds = targetRenderer.bounds;
            Vector3[] samplePoints = new Vector3[]
            {
                bounds.center,
                bounds.min,
                bounds.max,
                new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
                new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
                new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
            };

            int visibleCount = 0;

            foreach (var point in samplePoints)
            {
                Vector3 toPoint = point - spotlight.position;
                float distance = toPoint.magnitude;

                float angle = Vector3.Angle(spotlight.forward, toPoint);
                if (angle > spotLightComponent.spotAngle * 0.5f)
                    continue;

                if (distance > spotLightComponent.range)
                    continue;

                if (Physics.Raycast(spotlight.position, toPoint.normalized, out RaycastHit hit, distance))
                {
                    if (hit.transform == caster || hit.collider.transform.IsChildOf(caster.transform))
                    {
                        visibleCount++;
                    }
                }
                else
                {
                    visibleCount++;
                }
            }

            float visibilityRatio = (float)visibleCount / samplePoints.Length;
            return visibilityRatio >= visibilityThreshold;
        }

        private void Awake()
        {
            Enable();
        }

        private void Update()
        {
            if (!_isEnabled || HTJ21GameManager.IsPaused) return;

            if (HTJ21GameManager.Player && IsTargetVisibleInSpotlight(_spotLight.transform, _spotLight, HTJ21GameManager.Player.GetRenderer(), HTJ21GameManager.Player.transform, _visibilityThreshold))
            {
                OnPlayerHit.Invoke();
            }
        }
    }
}
