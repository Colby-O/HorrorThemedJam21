using PlazmaGames.Core.MonoSystem;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public interface IGPSMonoSystem : IMonoSystem
    {
        public void MoveTarget(Vector3 position);
        public Transform GetTarget();
        public Vector3 GetClosestNodePositionToPoint(List<Roadway> roadways, Vector3 target);
        public void TurnOn();
        public void TurnOff();
    }
}
