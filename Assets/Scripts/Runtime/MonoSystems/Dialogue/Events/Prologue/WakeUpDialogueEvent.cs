using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "WakeUpEvent", menuName = "Dialogue/Events/Prologue/WakeUp")]
    public class WakeUpDialogueEvent : DialogueEvent
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
                prologueDirector.WakeUpCutsceneLogic();
            }
        }

        public override void OnUpdate()
        {

        }
    }
}
