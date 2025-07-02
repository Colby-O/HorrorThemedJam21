using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using TMPro;
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

        public GameObject Icon { get; set; }

        public TMP_Text Text { get; set; }

        public bool IsDisabled { 
            get 
            { 
                return _isDisabled;
            } 
            set
            {
                _isDisabled = value;
                //targetGraphic.color = _isDisabled ? colors.disabledColor : colors.normalColor;

                ColorBlock cb = colors;

                if(_isDisabled)
                {
                    cb.highlightedColor = Color.white.SetA(1) * colors.disabledColor;
                    cb.normalColor = Color.white.SetA(1) * colors.disabledColor;
                    if (Text) Text.color = cb.normalColor;
                }
                else
                {
                    cb.highlightedColor = Color.white.SetA(1);
                    cb.normalColor = Color.white.SetA(0);
                    if (Text) Text.color = cb.disabledColor;
                }

                colors = cb;
            }
        }

        public Color GetDisabledColor()
        {
            return colors.disabledColor;
        }

        public bool IsPointerUsed { get; set; }

        public void ToggleSound(bool state) => _playerSound = state;

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
            EventSystem.current.SetSelectedGameObject(null);
            onPointerDown.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (IsDisabled) return;

            ColorBlock bc = colors;
            bc.normalColor = Color.white.SetA(1f);
            colors = bc;
            if (Icon) Icon.SetActive(true);
            if (Text) Text.color = colors.normalColor;
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (IsDisabled) return;

            ColorBlock bc = colors;
            bc.normalColor = Color.white.SetA(0f);
            colors = bc;
            if (Icon) Icon.SetActive(false);
            if (Text) Text.color = colors.disabledColor;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            ColorBlock bc = colors;
            bc.normalColor = Color.white.SetA(0f);
            colors = bc;
            if (Icon) Icon.SetActive(false);
        }
    }
}
