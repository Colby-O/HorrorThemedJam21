using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HTJ21
{
    public class EventTrigger : MonoBehaviour
    {
        [SerializeField] private string _eventName;
        [SerializeField] private bool _once = true;

        [SerializeField, ReadOnly] private bool _triggered = false;

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

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            _triggered = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }
    }
}
