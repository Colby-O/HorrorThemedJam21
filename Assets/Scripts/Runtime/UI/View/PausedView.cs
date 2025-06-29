using PlazmaGames.Core;
using PlazmaGames.Rendering.Blur;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace HTJ21
{
    public class PausedView : View
    {
        [SerializeField] private EventButton _resume;
        [SerializeField] private EventButton _settings;
        [SerializeField] private EventButton _exit;

        [SerializeField] List<GameObject> _icons;

        [Header("WebGL")]
        [SerializeField] private GameObject _blurredView;
        [SerializeField] private GameObject _view;

        public void Resume()
        {
            HTJ21GameManager.IsPaused = false;
            GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();

#if UNITY_WEBGL
            _blurredView.SetActive(false);
            _view.SetActive(true);
#else
            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().ToggleRendererFeature("Blur", false);
#endif
        }

        private void Settings()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<SettingsView>();
        }

        private void Exit()
        {
            Application.Quit();
        }

        public override void Init()
        {
            _resume.onPointerDown.AddListener(Resume);
            _settings.onPointerDown.AddListener(Settings);
            _exit.onPointerDown.AddListener(Exit);

            _resume.Icon = _icons[0];
            _settings.Icon = _icons[1];
            _exit.Icon = _icons[2];

            foreach (GameObject icon in _icons) icon.SetActive(false);
        }

        public override void Show()
        {
            base.Show();

#if UNITY_WEBGL
            _blurredView.SetActive(true);
            _view.SetActive(false);
#else

            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().ToggleRendererFeature("Blur", true);
#endif

            HTJ21GameManager.IsPaused = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
