using UnityEngine;

namespace HTJ21
{
    public class RockDragSound : MonoBehaviour
    {
        private Rigidbody _rig;
        private AudioSource _as;

        private void Start()
        {
            _rig = GetComponent<Rigidbody>();
            _as = GetComponent<AudioSource>();
        }

        private void FixedUpdate()
        {
            _as.volume = Mathf.Lerp(0, 1, _rig.linearVelocity.magnitude / HTJ21GameManager.Preferences.RockDragVolumeMaxSpeed);
        }
    }
}
