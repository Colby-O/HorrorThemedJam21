using PlazmaGames.DataPersistence;
using UnityEngine;

namespace HTJ21
{
    [System.Serializable]
    public class HTJ21GameData : GameData
    {
        public Act act;

        public bool xInvert = false;
        public bool yInvert = false;
        public float sensitivity = 0.5f;
        public float dialogueSpeed = 0.5f;
        public Language language = Language.EN;
        public int resolutionType = 0;
        public bool isFullscreen = true;
        public bool isVsyncOn = true;
    }
}
