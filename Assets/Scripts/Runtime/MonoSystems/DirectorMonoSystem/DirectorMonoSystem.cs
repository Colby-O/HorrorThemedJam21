using NUnit.Framework;
using PlazmaGames.Attribute;
using PlazmaGames.Core.Debugging;
using PlazmaGames.Runtime.DataStructures;
using System.Linq;
using UnityEngine;

namespace HTJ21
{
    public enum Act
    {
        Prologue,
        Act1,
        Home1,
        Act2,
        Home2,
        Act3,
        Epilogue
    }

    public class DirectorMonoSystem : MonoBehaviour, IDirectorMonoSystem
    {
        [SerializeField] private Act _startAct;

        [SerializeField, ReadOnly] SerializableDictionary<Act, Director> _directors;
        [SerializeField, ReadOnly] private Director _currentDirector;

        public void StartAct(Act act)
        {
            PlazmaDebug.LogWarning($"Starting Act {act} from {_currentDirector.GetAct()}.", "Director", 2, Color.purple);
            if (_currentDirector != null) _currentDirector.OnActEnd();
            _currentDirector = _directors[act];
            _currentDirector.OnActStart();
        }

        private void Start()
        {
            _directors = new SerializableDictionary<Act, Director>();
            Director[] directors = FindObjectsByType<Director>(FindObjectsSortMode.None);

            foreach (Director director in directors)
            {
                Act act = director.GetAct();
                if (_directors.ContainsKey(act))
                {
                    PlazmaDebug.LogWarning($"Trying to add {act} more than once. Ignoring duplicates.", "Director", 1, Color.yellow);
                    continue;
                }
                _directors.Add(act, director);
            }

            StartAct(_startAct);
        }

        private void Update()
        {
            _currentDirector.OnActUpdate();
        }
    }
}
