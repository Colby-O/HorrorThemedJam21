using UnityEngine;

namespace HTJ21
{
    public class WeatherMonoSystem : MonoBehaviour, IWeatherMonoSystem
    {
        private GameObject _weatherGameObject;

        private void Awake()
        {
            _weatherGameObject = GameObject.Instantiate(Resources.Load("Prefabs/Weather")) as GameObject;
        }

        private void Update()
        {
            if (HTJ21GameManager.Player != null) _weatherGameObject.transform.position = HTJ21GameManager.Player.transform.position;
        }
    }
}
