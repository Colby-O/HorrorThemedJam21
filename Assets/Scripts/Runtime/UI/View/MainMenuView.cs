using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.DataPersistence;
using PlazmaGames.UI;
using System.Collections.Generic;
using TMPro;
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

        [SerializeField] private TMP_Text _continueText;

        [SerializeField] List<GameObject> _icons;

        [SerializeField] private MainMenuCameraController _cameraController;

        public MainMenuCameraController GetCamera()
        {
            return _cameraController;
        }

        private void Continue()
        {
            HTJ21GameManager.IsPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Act startAct = GameManager.GetMonoSystem<IDirectorMonoSystem>().GetStartAct();

            if (startAct == Act.MainMenu || startAct == Act.Prologue)
            {
                _cameraController.GoToPlayer();
            }
            else
            {
                _cameraController.Disable();
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

            _continue.Icon = _icons[0];
            _newGame.Icon = _icons[1];
            _settings.Icon = _icons[2];
            _exit.Icon = _icons[3];

            foreach (GameObject icon in _icons) icon.SetActive(false);
        }

        public override void Show()
        {
            base.Show();

            _continue.IsDisabled = !GameManager.GetMonoSystem<IDataPersistenceMonoSystem>().IsGameLoaded();
            if (_continue.IsDisabled) _continueText.color = _continue.GetDisabledColor();
            else _continueText.color = Color.white;
            HTJ21GameManager.Player.LockMovement = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            HTJ21GameManager.UseCustomCursor();
        }
    }
}
