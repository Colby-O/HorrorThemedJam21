using PlazmaGames.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public class KeypadButton : MonoBehaviour, IClickable
    {
        public bool CanClick { get; set; }
        public UnityAction OnClick { get; set; }
        public UnityAction OnHoverEnter { get; set; }
        public UnityAction OnHoverExit { get; set; }

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

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

        private void Awake()
        {
            CanClick = true;
            OnHoverEnter += AddOutline;
            OnHoverExit += RemoveOutline;
        }
    }
}
