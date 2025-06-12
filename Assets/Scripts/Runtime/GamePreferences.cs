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

        [Header("Dialogue -- Act 1")]
        public string Act1Location;
        public DialogueSO IntroDialogue;
        public DialogueSO DriveTutorialDialogue;
        public DialogueSO PickupFlashlightDialogue;
    }
}
