using PlazmaGames.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public class PlayerInSpotlight : MonoBehaviour
    {
        [SerializeField] private Light _spotLight;
        [SerializeField] private float _visibilityThreshold = 0.5f;

        [SerializeField] private bool _useRaycast = true;
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

        private bool IsTargetVisibleInSpotlight(Transform spotlight, Light spotLightComponent, Transform target, Transform caster, float visibilityThreshold = 0.5f)
        {
            if (spotLightComponent.type != LightType.Spot)
                return false;

            int visibleCount = 0;


            Vector3 toPoint = target.position - spotlight.position;
            float distance = toPoint.magnitude;

            float angle = Vector3.Angle(spotlight.forward, toPoint);
            if (angle > spotLightComponent.spotAngle * 0.5f) return false;

            if (distance > spotLightComponent.range) return false;

            if (_useRaycast)
            {
                if (Physics.Raycast(spotlight.position, toPoint.normalized, out RaycastHit hit, distance))
                {
                    if (hit.transform != caster && !hit.collider.transform.IsChildOf(caster.transform))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void Awake()
        {
            Enable();
        }

        private void Update()
        {
            if (!_isEnabled || HTJ21GameManager.IsPaused) return;

            if (HTJ21GameManager.Player && IsTargetVisibleInSpotlight(_spotLight.transform, _spotLight, HTJ21GameManager.Player.transform, HTJ21GameManager.Player.transform, _visibilityThreshold))
            {
                OnPlayerHit.Invoke();
            }
        }
    }
}
