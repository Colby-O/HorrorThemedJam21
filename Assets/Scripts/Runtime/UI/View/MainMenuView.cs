using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.DataPersistence;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.UI;

namespace HTJ21
{
    public class MainMenuView : View
    {
        [SerializeField] private EventButton _continue;
        [SerializeField] private EventButton _newGame;
        [SerializeField] private EventButton _settings;
        [SerializeField] private EventButton _exit;

        [SerializeField] private MainMenuCameraController _cameraController;

        public MainMenuCameraController GetCamera()
        {
            return _cameraController;
        }

        private void Continue()
        {
            HTJ21GameManager.IsPaused = false;

            Act startAct = GameManager.GetMonoSystem<IDirectorMonoSystem>().GetStartAct();

            if (startAct == Act.MainMenu || startAct == Act.Prologue)
            {
                NewGame();
            }
            else
            {
                _cameraController.Disable();
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                GameManager.GetMonoSystem<IDirectorMonoSystem>().StartAct(GameManager.GetMonoSystem<IDirectorMonoSystem>().GetStartAct());
            }
        }

        private void NewGame()
        {
            GameManager.GetMonoSystem<IDataPersistenceMonoSystem>().DeleteGame();
            GameManager.GetMonoSystem<IDataPersistenceMonoSystem>().NewGame();
            GameManager.GetMonoSystem<IDataPersistenceMonoSystem>().LoadGame(true);

            HTJ21GameManager.IsPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _cameraController.GoToPlayer();
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
            _continue.onPointerDown.AddListener(Continue);
            _newGame.onPointerDown.AddListener(NewGame);
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

        private void Start()
        {
            _continue.IsDisabled = !GameManager.GetMonoSystem<IDataPersistenceMonoSystem>().IsGameLoaded();
        }
    }
}
