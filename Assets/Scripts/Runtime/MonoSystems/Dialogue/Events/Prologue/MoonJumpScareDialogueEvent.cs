using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "MoonJumpScareEvent", menuName = "Dialogue/Events/Prologue/MoonJumpScare")]
    public class MoonJumpScareDialogueEvent : DialogueEvent
    {
        public override void OnEnter()
        {

        }

        public override void OnExit()
        {
            Director director = GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentDirector();

            if (director.GetAct() == Act.Prologue)
            {
                PrologueDirector prologueDirector = director as PrologueDirector;
                prologueDirector.MoonJumpscare();
            }
        }

        public override void OnUpdate()
        {

        }
    }
}
