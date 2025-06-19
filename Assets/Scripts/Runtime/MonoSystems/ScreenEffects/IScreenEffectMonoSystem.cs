using PlazmaGames.Core.MonoSystem;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace HTJ21
{
    public interface IScreenEffectMonoSystem : IMonoSystem
    {
        public void ToggleRendererFeature(string featureName, bool state);
        public void ToggleRendererFeature(ScriptableRendererData rendererData, string featureName, bool state);
        public void SetStaticLevel(float scale);
        public void SetChromicOffset(float level);
        public void SetScreenBend(float bend);
        public void RestoreDefaults();
        void FadeToBlack(float time, System.Action then);
        public void TriggerBlink(float duration, float numBlinks, UnityAction onFinish = null, bool startFromOpen = true);
    }
}
