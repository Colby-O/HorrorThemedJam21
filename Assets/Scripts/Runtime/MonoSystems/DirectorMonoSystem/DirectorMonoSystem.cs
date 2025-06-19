using NUnit.Framework;
using PlazmaGames.Attribute;
using PlazmaGames.Core.Debugging;
using PlazmaGames.Runtime.DataStructures;
using System;
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

    public static class ActExtension
    {
        public static Act Next(this Act act)
        {
            Act[] acts = (Act[])Enum.GetValues(typeof(Act));
            int index = Array.IndexOf(acts, act);
            return acts[(index + 1) % acts.Length];
        }
    }

    public class DirectorMonoSystem : MonoBehaviour, IDirectorMonoSystem
    {
        [SerializeField] private Act _startAct;

        [Header("Map")]
        [SerializeField, ReadOnly] private HouseController _houseController;

        [SerializeField, ReadOnly] private SerializableDictionary<Act, Director> _directors;
        [SerializeField, ReadOnly] private Director _currentDirector;

        public void NextAct()
        {
            StartAct(_currentDirector.GetAct().Next());
        }

        public Director GetCurrentDirector()
        {
            return _currentDirector;
        }

        public Act GetCurrentAct()
        {
            return _currentDirector?.GetAct() ?? _startAct;
        }

        public void StartAct(Act act)
        {
            if (!_directors.ContainsKey(act)) return;

            if (_currentDirector != null)
            {
                PlazmaDebug.LogWarning($"Ending Act {_currentDirector.GetAct()}.", "Director", 2, Color.purple);
                _currentDirector.OnActEnd();
                _currentDirector.gameObject.SetActive(false);
            }

            PlazmaDebug.LogWarning($"Starting Act {act}.", "Director", 2, Color.purple);
            _currentDirector = _directors[act];
            _currentDirector.gameObject.SetActive(true);
            _currentDirector.OnActStart();

            if (_houseController) _houseController.OnActChange();
        }

        private void Start()
        {
            _houseController = FindAnyObjectByType<HouseController>();

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

                director.gameObject.SetActive(false);

                _directors.Add(act, director);
            }

            StartAct(_startAct);
        }

        private void Update()
        {
            if (_currentDirector) _currentDirector.OnActUpdate();
        }
    }
}
