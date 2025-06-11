using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "GamePreferences", menuName = "GamePreferences")]
    public class GamePreferences : ScriptableObject
    {
        public Language SelectedLanguage;
        public float ViewDistance = 50f;
        public float ComponentViewDistance = 25f;
        public float ComputeViewDistanceInterval = 1;
    }
}
