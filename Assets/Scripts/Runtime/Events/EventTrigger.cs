using System;
using System.Linq;
using System.Reflection;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class EventTrigger : MonoBehaviour
    {
        [SerializeField] private string _eventName;

        private void OnTriggerEnter(Collider other)
        {
            System.Type t = typeof(Events).Assembly.GetTypes().FirstOrDefault(t => t.Name == _eventName) ?? typeof(int);
            if (t == typeof(int)) return;
            ConstructorInfo con = t.GetConstructor(Type.EmptyTypes);
            if (con == null) return;
            GameManager.EmitEvent(con.Invoke(Array.Empty<object>()));
        }
    }
}
