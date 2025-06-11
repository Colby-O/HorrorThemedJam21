using PlazmaGames.Core;
using PlazmaGames.Rendering.Blur;
using PlazmaGames.UI;
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
            HTJ21GameManager.ToggleRendererFeature(HTJ21GameManager.MainRendererData, "Blur", false);
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
        }

        public override void Show()
        {
            base.Show();

#if UNITY_WEBGL
            _blurredView.SetActive(true);
            _view.SetActive(false);
#else

            HTJ21GameManager.ToggleRendererFeature(HTJ21GameManager.MainRendererData, "Blur", true);
#endif

            HTJ21GameManager.IsPaused = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
