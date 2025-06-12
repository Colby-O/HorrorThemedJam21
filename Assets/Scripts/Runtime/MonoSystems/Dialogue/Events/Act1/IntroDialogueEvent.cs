using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "IntroDialogueEvent", menuName = "Dialogue/Events/Intro")]
    public class IntroDialogueEvent : DialogueEvent
    {
        public override void OnEnter()
        {

        }

        public override void OnExit()
        {
            HTJ21GameManager.Car.GetComponent<CinematicCarController>().StopCinematic();
            HTJ21GameManager.IsPaused = false;
        }

        public override void OnUpdate()
        {

        }
    }
}
