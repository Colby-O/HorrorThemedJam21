using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "DreamGPSEvent", menuName = "Dialogue/Events/DreamGPS")]
    public class DreamGPSDialogueEvent : DialogueEvent
    {
        public override void OnEnter()
        {
            GameManager.GetMonoSystem<IGPSMonoSystem>().MoveTarget(FindAnyObjectByType<DreamDirector>().GetAfterLogTarget().position);
        }

        public override void OnExit()
        {

        }

        public override void OnUpdate()
        {

        }
    }
}
