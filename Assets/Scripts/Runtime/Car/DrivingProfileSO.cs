using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "DrivingProfile", menuName = "Car/New Driving Profile")]
    public class DrivingProfileSO : ScriptableObject
    {
        public float MaxThrottle = 1;
        public float MaxSpeed = 0;
        public bool Automatic = true;
        public float AutomaticUpShiftPoint;
        public float AutomaticDownShiftPoint;
        public float TorqueScale = 1;
    }
}
