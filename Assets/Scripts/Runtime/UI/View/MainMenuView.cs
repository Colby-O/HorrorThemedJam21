using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HTJ21
{
    public class MainMenuView : View
    {
        [SerializeField] private EventButton _play;
        [SerializeField] private EventButton _settings;
        [SerializeField] private EventButton _exit;

        private void Play()
        {
            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
            HTJ21GameManager.Car.GetComponent<CinematicCarController>().StopCinematic();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
            GameManager.EmitEvent(new Events.StartGame());
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
            GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(0, PlazmaGames.Audio.AudioType.Music, true);
            _play.onPointerDown.AddListener(Play);
            _settings.onPointerDown.AddListener(Settings);
            _exit.onPointerDown.AddListener(Exit);
        }

        public override void Show()
        {
            base.Show();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
