using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "DelayDialogueEvent", menuName = "Dialogue/Events/Delay")]
    public class DelayDialogueEvent : DialogueEvent
    {
        [SerializeField] private float _delay;
        private float _timer;

        public override void OnEnter()
        {
            _timer = 0f;
        }

        public override void OnExit()
        {

        }

        public override void OnUpdate()
        {
            _timer += Time.deltaTime;
        }

        public override bool CanProceed()
        {
            return _timer > _delay;
        }
    }
}
