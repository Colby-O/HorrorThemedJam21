using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "DreamSightingEvent", menuName = "Dialogue/Events/DreamSighting")]
    public class DreamSightingDialogueEvent : DialogueEvent
    {
        public override void OnEnter()
        {
            
        }

        public override void OnExit()
        {
            if (HTJ21GameManager.CarTutorial) HTJ21GameManager.CarTutorial.ShowTutorial(3);
            HTJ21GameManager.Car.Unlock();
        }

        public override void OnUpdate()
        {

        }
    }
}
