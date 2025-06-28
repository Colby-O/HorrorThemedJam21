using PlazmaGames.Attribute;
using PlazmaGames.Core;
using Unity.VisualScripting;
using UnityEngine;

namespace HTJ21
{
    public enum InspectType
    {
        Goto,
        ComeTo,
        Moveable,
        ReadableComeTo,
        ReadableGoTo
    }

    public class InspectableItem : MonoBehaviour, IInteractable
    {
        [SerializeField] private InspectType _inspectType;
        [SerializeField] private bool _rotateWithPlayer = false;
        [SerializeField] private Transform _offset;
        [SerializeField] private Transform _targetOverride;
        [SerializeField] private float _comeToOffsetOverride;

        [SerializeField] private string _text;

        [Header("Dialogue")]
        [SerializeField] private bool _playMoreThanOnce = false;
        [SerializeField] private DialogueSO _onInspectDialogue;
        [SerializeField, ReadOnly] private bool _hasLookedAtOnce = false;

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

        private Vector3 _startPos;
        private Quaternion _startRot;

        public bool CanInteract { get; set; }

        public void SetDialogue(DialogueSO so)
        {
            _onInspectDialogue = so;
        }

        public DialogueSO GetDialogue()
        {
            return _onInspectDialogue;
        }

        public void EndInteraction()
        {

        }

        public string GetHint()
        {

            return
                $"Click 'E' to {(HTJ21GameManager.Inspector.IsInspecting() ? "stop" : "")} {((_inspectType == InspectType.Moveable) ? "pickup" : "inspect")}{(HTJ21GameManager.Inspector.IsInspecting() ? "ing" : "")}";
        }

        public bool Interact(Interactor interactor)
        {
            if (_playMoreThanOnce || !_hasLookedAtOnce)
            {
                _hasLookedAtOnce = true;
                if (_onInspectDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_onInspectDialogue);
            }

            if (HTJ21GameManager.Inspector) HTJ21GameManager.Inspector.StartInspect(transform, _inspectType, _rotateWithPlayer, _offset, _targetOverride, _text, _comeToOffsetOverride);
            return true;
        }

        public bool IsInteractable()
        {
            return CanInteract;
        }

        public void AddOutline()
        {
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
            if (!_outlineMR) return;

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
            CanInteract = true;
            _startPos = transform.position;
            _startRot = transform.rotation;
        }

        private void LateUpdate()
        {
            if (HTJ21GameManager.Inspector.IsInspecting() && _hasOutline) RemoveOutline();
        }

        public void Restart()
        {
            CanInteract = true;
            transform.position = _startPos;
            transform.rotation = _startRot;
        }
    }
}
