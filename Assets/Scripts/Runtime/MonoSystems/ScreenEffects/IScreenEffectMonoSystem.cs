using System;
using PlazmaGames.Core.MonoSystem;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace HTJ21
{
    public interface IScreenEffectMonoSystem : IMonoSystem
    {
        public void ToggleRendererFeature(string featureName, bool state);
        public void ToggleRendererFeature(ScriptableRendererData rendererData, string featureName, bool state);
        public void SetStaticLevel(float scale);
        public void SetChromicOffset(float level);
        public void RestoreDefaults();
        void FadeToBlack(float time, System.Action then);
    }
}
