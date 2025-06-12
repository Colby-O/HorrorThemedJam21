using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "DreamSightingEvent", menuName = "Dialogue/Events/DreamSighting")]
    public class DreamSightingDialogueEvent : DialogueEvent
    {
        public override void OnEnter()
        {
            HTJ21GameManager.Car.Unlock();
        }

        public override void OnExit()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}
