using PlazmaGames.Attribute;
using PlazmaGames.Core.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    [Serializable]
    public struct KeypadPattern
    {
        public bool E00, E01, E02, E10, E11, E12, E20, E21, E22;

        public KeypadPattern(bool E00, bool E01, bool E02, bool E10, bool E11, bool E12, bool E20, bool E21, bool E22)
        {
            this.E00 = E00;
            this.E01 = E01;
            this.E02 = E02;
            this.E10 = E10;
            this.E11 = E11;
            this.E12 = E12;
            this.E20 = E20;
            this.E21 = E21;
            this.E22 = E22;
        }

        public bool Get(int i)
        {
            if (i == 0) return E00;
            if (i == 1) return E01;
            if (i == 2) return E02;
            if (i == 3) return E10;
            if (i == 4) return E11;
            if (i == 5) return E12;
            if (i == 6) return E20;
            if (i == 7) return E21;
            if (i == 8) return E22;
            return false;
        }

        public void Set(int i, bool state)
        {
            if (i == 0) E00 = state;
            if (i == 1) E01 = state;
            if (i == 2) E02 = state;
            if (i == 3) E10 = state;
            if (i == 4) E11 = state;
            if (i == 5) E12 = state;
            if (i == 6) E20 = state;
            if (i == 7) E21 = state;
            if (i == 8) E22 = state;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is KeypadPattern)) return false;
            KeypadPattern other = (KeypadPattern)obj;
            return
                this.E00 == other.E00 &&
                this.E01 == other.E01 &&
                this.E02 == other.E02 &&
                this.E10 == other.E10 &&
                this.E11 == other.E11 &&
                this.E12 == other.E12 &&
                this.E20 == other.E20 &&
                this.E21 == other.E21 &&
                this.E22 == other.E22;
        }
    }

    public class Keypad : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private List<MeshRenderer> _lights;
        [SerializeField] private List<KeypadButton> _buttons;
        [SerializeField] private Door _linkedDoor;

        [Header("Pattern")]
        [SerializeField, Range(0, 7)] private int _minNumbers = 4;
        [SerializeField, Range(0, 7)] private int _maxNumbers = 7;
        [SerializeField] private KeypadPattern _correctPattern;
        [SerializeField, ReadOnly] private KeypadPattern _currentPattern;

        [SerializeField, ReadOnly] private bool _isSolved;

        public UnityEvent OnSolved {get; set;}

        public KeypadPattern GetCorrectPattern() => _correctPattern;

        public void Restart()
        {
            _isSolved = false;
            _currentPattern = new KeypadPattern();
            DisplayCurrentPattern();

            if (TryGetComponent(out InspectableItem item)) item.CanInteract = true;
            foreach (KeypadButton button in _buttons) button.Enable();
        }

        private KeypadPattern GenerateRandomPasscode()
        {
            List<int> numbers = Enumerable.Range(0, 9).ToList(); 
            System.Random rng = new System.Random();
            numbers = numbers.OrderBy(x => rng.Next()).ToList(); 
            List<int> result = numbers.Take(UnityEngine.Random.Range(_minNumbers, _maxNumbers + 1)).ToList();

            KeypadPattern pattern = new KeypadPattern(false, false, false, false, false, false, false, false, false);

            foreach (int i in result)
            {
                pattern.Set(i, true);
            }

            return pattern;
        }

        private void DisplayCurrentPattern()
        {
            if (_lights.Count < 9)
            {
                PlazmaDebug.LogWarning("Not enough lights were assigned. Ignoring display request.", "Keypad", 1, Color.yellow);
                return;
            }

            for (int i = 0; i < 9; i++) 
            {
                MeshRenderer light = _lights[i];
                if (_currentPattern.Get(i)) light.material.SetColor("_BaseColor", Color.red);
                else light.material.SetColor("_BaseColor", Color.black);
            }
        }

        private void OnButtonClicked(int id)
        {
            _currentPattern.Set(id, !_currentPattern.Get(id));
            DisplayCurrentPattern();
        }

        private bool IsSolved()
        {
            return _currentPattern.Equals( _correctPattern);
        }

        private void Solve()
        {
            foreach (KeypadButton button in _buttons) button.Disable();
            for (int i = 0; i < 9; i++)
            {
                MeshRenderer light = _lights[i];
                if (_currentPattern.Get(i)) light.material.SetColor("_BaseColor", Color.green);
            }
            OnSolved.Invoke();
            _isSolved = true;
        }

        private void Awake()
        {
            OnSolved = new UnityEvent();
            _isSolved = false;

            if (_buttons.Count < 9)
            {
                PlazmaDebug.LogWarning("Not enough buttons were assigned.", "Keypad", 1, Color.yellow);
                return;
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    int id = i;
                    _buttons[i].OnClick += () => OnButtonClicked(id);
                }
            }

            _correctPattern = GenerateRandomPasscode();

            if (_linkedDoor)
            {
                _linkedDoor.Lock();
                OnSolved.AddListener(_linkedDoor.Unlock);
            }

            DisplayCurrentPattern();
        }

        private void Update()
        {
            if (!_isSolved && IsSolved()) Solve();
        }
    }
}
