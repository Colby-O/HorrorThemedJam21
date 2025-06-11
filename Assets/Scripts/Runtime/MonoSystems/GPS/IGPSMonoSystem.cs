using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace HTJ21
{
    public interface IGPSMonoSystem : IMonoSystem
    {
        public void MoveTarget(Vector3 position);
        public Transform GetTarget();
        public void TurnOn();
        public void TurnOff();
    }
}
