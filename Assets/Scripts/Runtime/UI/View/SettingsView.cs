using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HTJ21
{
    public class SettingsView : View
    {
        [SerializeField] private EventButton _back;

        [SerializeField] private Scrollbar _overall;
        [SerializeField] private Scrollbar _music;
        [SerializeField] private Scrollbar _sfx;

        [SerializeField] private RectTransform _overallFill;
        [SerializeField] private RectTransform _musicFill;
        [SerializeField] private RectTransform _sfxFill;

        private void OnOverallChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetOverallVolume(val);
            _overallFill.localScale = new Vector3(val, _overallFill.localScale.y, _overallFill.localScale.z);
        }

        private void OnMusicChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetMusicVolume(val);
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetAmbientVolume(val);
            _musicFill.localScale = new Vector3(val, _musicFill.localScale.y, _musicFill.localScale.z);
        }

        private void OnSfXChanged(float val)
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().SetSfXVolume(val);
            _sfxFill.localScale = new Vector3(val, _sfxFill.localScale.y, _sfxFill.localScale.z);
        }

        private void Back()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
        }

        public override void Init()
        {
            _back.onPointerDown.AddListener(Back);

            _overall.onValueChanged.AddListener(OnOverallChanged);
            _music.onValueChanged.AddListener(OnMusicChanged);
            _sfx.onValueChanged.AddListener(OnSfXChanged);

            _overall.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetOverallVolume();
            _music.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetMusicVolume();
            _sfx.value = GameManager.GetMonoSystem<IAudioMonoSystem>().GetSfXVolume();

            _overallFill.localScale = new Vector3(_overall.value, _overallFill.localScale.y, _overallFill.localScale.z);
            _musicFill.localScale = new Vector3(_music.value, _musicFill.localScale.y, _musicFill.localScale.z);
            _sfxFill.localScale = new Vector3(_sfx.value, _sfxFill.localScale.y, _sfxFill.localScale.z);
        }
    }
}
