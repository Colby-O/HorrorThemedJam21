using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "HeadlightDialogueEvent", menuName = "Dialogue/Events/HeadlightTutorialTrigger")]
    public class HeadlightDialogueEvent : DialogueEvent
    {
        public override void OnEnter()
        {
            if (HTJ21GameManager.CarTutorial) HTJ21GameManager.CarTutorial.ShowTutorial(1);
        }

        public override void OnExit()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}
