using UnityEngine;
using UnityEngine.SceneManagement;

namespace HTJ21
{
    public class WeatherMonoSystem : MonoBehaviour, IWeatherMonoSystem
    {
        private GameObject _weatherGameObject;

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            _weatherGameObject = GameObject.Instantiate(Resources.Load("Prefabs/Weather")) as GameObject;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }

        private void Update()
        {
            if (HTJ21GameManager.Player != null && _weatherGameObject != null) _weatherGameObject.transform.position = HTJ21GameManager.CurrentControlable.transform.position;
        }
    }
}
