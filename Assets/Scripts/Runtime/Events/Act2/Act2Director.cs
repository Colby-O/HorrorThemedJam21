using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HTJ21
{
    public class Act2Director : MonoBehaviour
    {
        private void StartAct()
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player) player.EnablePlayer();

            GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
            HTJ21GameManager.IsPaused = false;

            GameManager.GetMonoSystem<IUIMonoSystem>().GetView<GameView>().SkipLocation();
            GameManager.GetMonoSystem<IDialogueMonoSystem>().ResetDialogue();
            GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOff();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().DisableThunder();
            GameManager.GetMonoSystem<IWeatherMonoSystem>().DisableRain();
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
           
        }

        private void Start()
        {
            StartAct();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }
    }
}
