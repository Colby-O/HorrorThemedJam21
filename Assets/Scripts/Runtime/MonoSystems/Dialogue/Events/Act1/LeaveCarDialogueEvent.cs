using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "LeaveCarEvent", menuName = "Dialogue/Events/Act1/LeaveCar")]
    public class LeaveCarDialogueEvent : DialogueEvent
    {
        public override void OnEnter()
        {

        }

        public override void OnExit()
        {
            HTJ21GameManager.CarTutorial.ShowTutorial(3);
            HTJ21GameManager.Car.Unlock();
        }

        public override void OnUpdate()
        {

        }
    }
}
