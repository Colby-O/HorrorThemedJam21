using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public class KeypadButton : MonoBehaviour, IClickable
    {
        [Header("Audio")]
        [SerializeField] private AudioClip _clickClip;

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

        public bool CanClick { get; set; }
        public UnityAction OnClick { get; set; }
        public UnityAction OnHoverEnter { get; set; }
        public UnityAction OnHoverExit { get; set; }

        public void Disable()
        {
            CanClick = false;
            Material[] mats = _outlineMR.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetInt("Boolean_8BBF99CD", 1);
            }
        }

        public void AddOutline()
        {
            if (!_outlineMR || _hasOutline || !CanClick) return;

            _hasOutline = true;
            Material[] mats = _outlineMR.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetInt("Boolean_8BBF99CD", 0);
            }
            _outlineMR.materials = mats;
        }

        public void RemoveOutline()
        {
            if (!_outlineMR || !_hasOutline || !CanClick) return;

            _hasOutline = false;
            Material[] mats = _outlineMR.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetInt("Boolean_8BBF99CD", 1);
            }
            _outlineMR.materials = mats;
        }

        private void PlayClick()
        {
            if (_clickClip) GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_clickClip, PlazmaGames.Audio.AudioType.Sfx, false, true);
        }

        private void Awake()
        {
            CanClick = true;
            OnClick += PlayClick;
            OnHoverEnter += AddOutline;
            OnHoverExit += RemoveOutline;
        }
    }
}
