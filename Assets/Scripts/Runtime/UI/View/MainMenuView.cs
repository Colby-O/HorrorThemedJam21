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

        [SerializeField] private MainMenuCameraController _cameraController;

        public MainMenuCameraController GetCamera()
        {
            return _cameraController;
        }

        private void Play()
        {
            HTJ21GameManager.IsPaused = false;
            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);
            //GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(1, PlazmaGames.Audio.AudioType.Music, true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
            _cameraController.GoToPlayer();
            //HTJ21GameManager.IsPaused = false;
            //GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().ShowLocation(
            //    HTJ21GameManager.Preferences.Act1Location, 
            //    () => GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(HTJ21GameManager.Preferences.IntroDialogue)
            //);

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
            HTJ21GameManager.Player.LockMovement = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}
