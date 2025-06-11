using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "GamePreferences", menuName = "GamePreferences")]
    public class GamePreferences : ScriptableObject
    {
        public Language SelectedLanguage;
    }
}
