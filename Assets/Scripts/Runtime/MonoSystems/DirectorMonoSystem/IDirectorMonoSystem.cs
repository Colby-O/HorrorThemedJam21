using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace HTJ21
{
    public interface IDirectorMonoSystem : IMonoSystem
    {
        public void Begin();
        public void NextAct();
        public bool IsCurrentActIndoors();
        public Director GetCurrentDirector();
        public Act GetCurrentAct();
        public void StartAct(Act act);
    }
}
