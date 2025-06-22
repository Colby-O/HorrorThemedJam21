using System.Linq;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "GpsUpdate", menuName = "Dialogue/Events/Act1/GpsUpate")]
    public class GpsUpdateDialogueEvent : DialogueEvent
    {
        public string NewTarget;
        public bool GpsState;
        public float Delay;

        private float _startTime = 0;
        
        public override void OnEnter()
        {
            _startTime = Time.time;
        }

        private void Set()
        {
            IGPSMonoSystem gps = GameManager.GetMonoSystem<IGPSMonoSystem>();
            if (GpsState == false)
            {
                gps.TurnOff();
            }
            else
            {
                gps.TurnOn();
                if (NewTarget != "")
                {
                    GameObject target = GameObject.FindGameObjectsWithTag("GpsMarker").FirstOrDefault(g => g.name == NewTarget);
                    if (target)
                    {
                        gps.MoveTarget(target.transform.position);
                    }
                }
            }
        }

        public override void OnExit()
        {
        }

        public override void OnUpdate()
        {
            if (_startTime > 0 && Time.time - _startTime > Delay)
            {
                _startTime = 0;
                Set();
            }
        }
    }
}
