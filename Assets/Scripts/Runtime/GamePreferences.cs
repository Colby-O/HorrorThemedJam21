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
        
        [Header("Lune")]
        public Color MoonRedColor;
        
        [Header("Rope")]
        public float RopeSnapDistance = 3;
        public AudioClip RopeSnapSound;
        public float RockDragVolumeMaxSpeed = 1;
        public float SoccerBallHitForce = 1;
        public AudioClip BoardPopSound;

        [Header("Dialogue -- Act 1")]
        public string Act1Location;
        public DialogueSO IntroDialogue;
        public DialogueSO PickupFlashlightDialogue;

        public float HangingHitForce = 0.1f;
    }
}
