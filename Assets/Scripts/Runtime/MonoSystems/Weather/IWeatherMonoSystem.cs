using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace HTJ21
{
    public interface IWeatherMonoSystem : IMonoSystem
    {
        public void SpawnLightingAt(Vector3 pos);
        public void DisableThunder();
        public void EnableThunder();
        public void EnableRain();
        public void DisableRain();
        public void SetRainState(bool isIndoors);
    }
}
