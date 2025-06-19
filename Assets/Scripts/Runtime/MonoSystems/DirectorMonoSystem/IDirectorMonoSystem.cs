using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace HTJ21
{
    public interface IDirectorMonoSystem : IMonoSystem
    {
        public void NextAct();
        public Director GetCurrentDirector();
        public Act GetCurrentAct();
        public void StartAct(Act act);
    }
}
