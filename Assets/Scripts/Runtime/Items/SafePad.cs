using PlazmaGames.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public class SafePad : MonoBehaviour
    {
        [SerializeField] private List<MeshRenderer> _lights;
        [SerializeField] private List<KeypadButton> _buttons;
        [SerializeField] private Door _linkedDoor;

        [SerializeField] private List<int> _passcode;

        [SerializeField, ReadOnly] List<int> _entered;
        [SerializeField, ReadOnly] private bool _isSolved;

        public UnityEvent OnSolved {  get; set; }

        private void Failed()
        {
            _entered.Clear();

            foreach (MeshRenderer mr in  _lights) mr.material.SetColor("_BaseColor", Color.black);
        }

        private void OnButtonClicked(int id)
        {
            _entered.Add(id);
            if (_entered.Count <= 3) _lights[_entered.Count - 1].material.SetColor("_BaseColor", Color.red);
        }

        private bool IsSolved()
        {
            return _passcode.SequenceEqual(_entered);
        }

        private void Solve()
        {
            _isSolved = true;
            OnSolved?.Invoke();
            if (TryGetComponent(out InspectableItem item)) item.CanInteract = false;
            foreach (KeypadButton button in _buttons) button.Disable();
            foreach (MeshRenderer mr in _lights) mr.material.SetColor("_BaseColor", Color.green);
        }

        private void Awake()
        {
            _isSolved = false;
            OnSolved = new UnityEvent();
            for (int i = 0; i < _buttons.Count; i++)
            {
                int id = i;
                _buttons[i].OnClick += () => OnButtonClicked(id);
            }
            if (_linkedDoor)
            {
                _linkedDoor.Lock();
                OnSolved.AddListener(() => {
                    _linkedDoor.Unlock();
                    _linkedDoor.Open(HTJ21GameManager.Player.transform);
                    HTJ21GameManager.Inspector.EndInspect();
                });
            }

            Failed();
        }

        private void Update()
        {
            if (!_isSolved && IsSolved())
            {
                Solve();
            }
            else if (_entered.Count >= 3) Failed();
        }
    }
}
