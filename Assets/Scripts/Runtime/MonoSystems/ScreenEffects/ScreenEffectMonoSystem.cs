using System;
using System.Collections.Generic;
using System.Linq;
using PlazmaGames.Attribute;
using PlazmaGames.Core.Debugging;
using PlazmaGames.Rendering.CRT;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField] private float _defaultScreenBend = 10000f; 

        // Fade to black parameters
        private bool _fadeToBlack = false;
        private float _fadeToBlackStartTime;
        private float _fadeToBlackLength;
        private System.Action _fadeToBlackCallback;

        [SerializeField, ReadOnly] private UnityEngine.UI.Image _fadeToBlackImage;

        [SerializeField, ReadOnly] private GameObject _moonScreen;
        [SerializeField, ReadOnly] private TMP_Text[] _moonTexts;

        // Moon scare properties 
        private bool _moon = false;
        private float _moonDuration;
        private float _moonStartTime;
        private UnityAction _onMoonFinished;

        // Blink parameters
        private bool _startFromOpen = true;
        private bool _blink = false;
        private float _blinkStartTime;
        private float _blinkDuration;
        private int _numBlinks;
        private UnityAction _blinkCallback;

        public void ShowMoon(float duration, int textId, UnityAction onFinish = null)
        {
            _moonScreen.SetActive(true);
            for (int i = 0; i < _moonTexts.Length; i++) 
            {
                if (i == textId) _moonTexts[i].gameObject.SetActive(true);
                else _moonTexts[i].gameObject.SetActive(false);
            }

            _moon = true;
            _moonDuration = duration;
            _onMoonFinished = onFinish;
            _moonStartTime = Time.time;
        }

        public void HideMoon()
        {
            _moonScreen.SetActive(false);
            _moon = false;
            _moonDuration = 0f;
            _onMoonFinished = null;
        }

        public void FadeToBlack(float time, System.Action then)
        {
            _fadeToBlack = true;
            _fadeToBlackCallback = then;
            _fadeToBlackLength = time;
            _fadeToBlackStartTime = Time.time;
        }

        public void TriggerBlink(float duration, float numBlinks, UnityAction onFinish = null, bool startFromOpen = true)
        {
            _blink = true;
            _startFromOpen = startFromOpen;
            _blinkDuration = duration;
            _numBlinks = Mathf.RoundToInt(numBlinks * 2);
            _blinkCallback = onFinish;
            _blinkStartTime = Time.time;
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
            PlazmaDebug.Log($"Setting renderer feature '{featureName}' to an {state} state.", "Screen Effect", verboseLevel: 2, color: Color.green);
            ToggleRendererFeature(_rendererData, featureName, state);
        }

        public void SetStaticLevel(float scale)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' noise scale to {scale}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetNoiseLevel(scale);
            else PlazmaDebug.LogWarning($"Setting renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }

        public void SetChromicOffset(float level)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' chromic offset to {level}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetChromicOffset(level);
            else PlazmaDebug.LogWarning($"Renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }

        public void SetScreenBend(float bend)
        {
            PlazmaDebug.Log($"Setting renderer feature '{"CRTRendererFeature"}' screen bend to {bend}.", "Screen Effect", verboseLevel: 2, color: Color.green);
            CRTRendererFeature crt = GetRendererFeature<CRTRendererFeature>(_rendererData, "CRTRendererFeature");
            if (crt) crt.SetScreenBend(bend);
            else PlazmaDebug.LogWarning($"Renderer feature '{"CRTRendererFeature"}' is null.", "Screen Effect", verboseLevel: 2, color: Color.yellow);
        }

        public void RestoreDefaults()
        {
            SetStaticLevel(_defaultNoiseScale);
            SetChromicOffset(_defaultChromicOffset);
            SetScreenBend(_defaultScreenBend);
            ToggleRendererFeature("Blur", false);
            if (_moonScreen) _moonScreen.SetActive(false);
            if (_fadeToBlackImage)
            {
                _fadeToBlackImage.color = _fadeToBlackImage.color.SetA(0);
                _fadeToBlack = false;
                _fadeToBlackCallback = null;
            }
        }
        
        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoad;
        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoad;

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            RestoreDefaults();
            GameObject i = GameObject.FindWithTag("FadeToBlackScreen");
            _fadeToBlackImage = i.transform.GetComponent<UnityEngine.UI.Image>();
        }

        public void Start()
        {
            GameObject i = GameObject.FindWithTag("FadeToBlackScreen");
            _fadeToBlackImage = i.transform.GetComponent<UnityEngine.UI.Image>();

            _moonScreen = GameObject.FindWithTag("MoonFlash");
            _moonTexts = _moonScreen.GetComponentsInChildren<TMP_Text>();

            RestoreDefaults();
        }

        private void Update()
        {
            if (HTJ21GameManager.IsPaused) return;

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
            else if (_blink)
            {
                float t = ((Time.time - _blinkStartTime) / _blinkDuration);
                float bend = (_startFromOpen) ? 1f - Mathf.PingPong(t * _numBlinks, 1f) : Mathf.PingPong(t * _numBlinks, 1f);
                SetScreenBend(bend);
                if (t >= 1f)
                {
                    _blink = false;
                    _startFromOpen = true;
                    _blinkCallback?.Invoke();
                    _blinkCallback = null;
                    //SetScreenBend(_defaultScreenBend);
                }
            }
            else if (_moon)
            {
                float t = (Time.time - _moonStartTime) / _moonDuration;
                if (t >= 1)
                {
                    _moon = false;
                    _onMoonFinished?.Invoke();
                    _onMoonFinished = null;
                    _moonScreen.SetActive(false);
                }
            }
        }
    }
}
