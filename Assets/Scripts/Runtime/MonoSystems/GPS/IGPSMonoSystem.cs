using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace HTJ21
{
    public interface IGPSMonoSystem : IMonoSystem
    {
        public void TurnOn();
        public void TurnOff();
    }
}
