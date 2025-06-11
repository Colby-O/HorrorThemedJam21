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
        [SerializeField] private bool _once = true;

        private bool _triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (!(other.CompareTag("Player") || other.CompareTag("Car"))) return;
            if (_triggered && _once) return;
            _triggered = true;
            System.Type t = typeof(Events).Assembly.GetTypes().FirstOrDefault(t => t.Name == _eventName) ?? typeof(int);
            if (t == typeof(int)) return;
            ConstructorInfo con = t.GetConstructor(Type.EmptyTypes);
            if (con == null) return;
            GameManager.EmitEvent(con.Invoke(Array.Empty<object>()));
        }
    }
}
