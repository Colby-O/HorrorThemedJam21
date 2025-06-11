using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace HTJ21
{
    public interface IWeatherMonoSystem : IMonoSystem
    {
        public void SpawnLightingAt(Vector3 pos);
    }
}
