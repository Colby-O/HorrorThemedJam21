using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HTJ21
{
    public class EventButton : Button, IPointerDownHandler, IPointerUpHandler
    {
        public UnityEvent onPointerUp = new UnityEvent();
        public UnityEvent onPointerDown = new UnityEvent();

        [SerializeField] private bool _playerSound = true;
        [SerializeField, ReadOnly] private bool _isDisabled = false;

        private bool _isHovering = false;

        public bool IsDisabled { 
            get 
            { 
                return _isDisabled;
            } 
            set
            {
                _isDisabled = value;
                targetGraphic.color = _isDisabled ? colors.disabledColor : colors.normalColor;
            }
        }

        public bool IsPointerUsed { get; set; }

        public void ToggleSound(bool state) => _playerSound = state;

        public void ForceHighlightedt(bool state)
        {
            if (!IsPointerUsed) targetGraphic.color = state ? colors.pressedColor : colors.normalColor;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (IsDisabled) return;

            IsPointerUsed = false;
            base.OnPointerUp(eventData);
            onPointerUp.Invoke();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (IsDisabled) return;

            if (_playerSound) GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(0, PlazmaGames.Audio.AudioType.Sfx, false, true);
            IsPointerUsed = true;
            base.OnPointerDown(eventData);
            onPointerDown.Invoke();
        }
    }
}
