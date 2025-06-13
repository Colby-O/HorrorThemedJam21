using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "ReverseTutorialDialogueEvent", menuName = "Dialogue/Events/ReverseTutorial")]
    public class ReverseTutorialDialogueEvent : DialogueEvent
    {
        public override void OnEnter()
        {
            if (HTJ21GameManager.CarTutorial) HTJ21GameManager.CarTutorial.ShowTutorial(2);
        }

        public override void OnExit()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}
