using System;
using System.Linq;
using System.Reflection;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "CallEvent", menuName = "Dialogue/Events/CallEvent")]
    public class CallEventDialogueEvent : DialogueEvent
    {
        public string EventName;

        public override void OnEnter()
        {
        }

        public override void OnExit()
        {
            System.Type t = typeof(Events).Assembly.GetTypes().FirstOrDefault(t => t.Name == EventName) ?? typeof(int);
            if (t == typeof(int)) return;
            ConstructorInfo con = t.GetConstructor(Type.EmptyTypes);
            if (con == null) return;
            GameManager.EmitEvent(con.Invoke(Array.Empty<object>()));
        }

        public override void OnUpdate()
        {

        }

        public override bool CanProceed()
        {
            return true;
        }
    }
}
