using System;
using PlazmaGames.Core.Debugging;
using PlazmaGames.Rendering.CRT;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace HTJ21
{
    public class ScreenEffectMonoSystem : MonoBehaviour, IScreenEffectMonoSystem
    {
        [SerializeField] UniversalRendererData _rendererData;

        [Header("Default CRT Values")]
        [SerializeField] private float _defaultNoiseScale = 0f;
        [SerializeField] private float _defaultChromicOffset = 0f;

        private bool _fadeToBlack = false;
        private float _fadeToBlackStartTime;
        private float _fadeToBlackLength;
        private System.Action _fadeToBlackCallback;

        private UnityEngine.UI.Image _fadeToBlackImage;
        

        public void FadeToBlack(float time, System.Action then)
        {
            _fadeToBlack = true;
            _fadeToBlackCallback = then;
            _fadeToBlackLength = time;
            _fadeToBlackStartTime = Time.time;
        }

        private T GetRendererFeature<T>(ScriptableRendererData rendererData, string featureName) where T : ScriptableRendererFeature
        {
            foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
            {
                if (feature != null && feature.name == featureName)
                {
                    return feature as T;
                }
            }

            return null;
        }

        public void ToggleRendererFeature(ScriptableRendererData rendererData, string featureName, bool state)
        {
            ScriptableRendererFeature feature = GetRendererFeature<ScriptableRendererFeature>(rendererData, featureName);
            if (feature != null) feature.SetActive(state);
        }

        public void ToggleRendererFeature(string featureName, bool state)
        {
            PlazmaDebug.Log($"Setting renderer feature '{featureName}' to an {state} state.", "ScreenEffectMonoSystem", verboseLevel: 2, color: Color.green);
            ToggleRendererFeature(_rendererData, featureName, state);
        }

        public void SetStaticLevel(float scale)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' noise scale to {scale}.", "ScreenEffectMonoSystem", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetNoiseLevel(scale);
            else PlazmaDebug.LogWarning($"Setting renderer feature '{"CRTRendererFeature"}' is null.", "ScreenEffectMonoSystem", verboseLevel: 2, color: Color.yellow);
        }

        public void SetChromicOffset(float level)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' chromic offset to {level}.", "ScreenEffectMonoSystem", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetChromicOffset(level);
            else PlazmaDebug.LogWarning($"Setting renderer feature '{"CRTRendererFeature"}' is null.", "ScreenEffectMonoSystem", verboseLevel: 2, color: Color.yellow);
        }

        public void RestoreDefaults()
        {
            SetStaticLevel(_defaultNoiseScale);
            SetChromicOffset(_defaultChromicOffset);
            ToggleRendererFeature("Blur", false);
            _fadeToBlackImage.color = _fadeToBlackImage.color.SetA(0);
        }
        
        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoad;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoad;

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            RestoreDefaults();
            GameObject i = GameObject.FindWithTag("FadeToBlackScreen");
            _fadeToBlackImage = i.transform.GetComponent<UnityEngine.UI.Image>();
        }

        private void Update()
        {
            if (_fadeToBlack)
            {
                float t = (Time.time - _fadeToBlackStartTime) / _fadeToBlackLength;
                _fadeToBlackImage.color = _fadeToBlackImage.color.SetA(Mathf.Clamp01(t));
                if (t >= 1)
                {
                    _fadeToBlack = false;
                    _fadeToBlackCallback();
                    _fadeToBlackCallback = null;
                }
            }
        }

        public void Start()
        {
            GameObject i = GameObject.FindWithTag("FadeToBlackScreen");
            _fadeToBlackImage = i.transform.GetComponent<UnityEngine.UI.Image>();
            RestoreDefaults();
        }
    }
}
