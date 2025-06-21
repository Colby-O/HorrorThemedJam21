using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class Door : MonoBehaviour, IInteractable
    {
        [Header("References")]
        [SerializeField] private Transform _pivot;
        [SerializeField] private Transform _center;

        [Header("Settings")]
        [SerializeField] private float _openSpeed = 1.5f;
        [SerializeField] private int _directionOverride = 0;

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

        [Header("Dialogue")]
        [SerializeField] private DialogueSO _onFirstTryLockedDialogue;
        [SerializeField] private DialogueSO _onLockedDialogue;
        [SerializeField, ReadOnly] private bool _hasAttemptedToUnlock = false;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        [SerializeField] private AudioClip _lockedSound;

        [SerializeField, ReadOnly] private bool _isOpen = false;
        [SerializeField, ReadOnly] private bool _inProgress = false;

        [SerializeField] private bool _isLocked = false;

        private float CurrentAngle()
        {
            float angle = _pivot.localRotation.eulerAngles.y;
            angle %= 360;
            angle = angle > 180 ? angle - 360 : angle;
            return angle;
        }

        public void Lock()
        {
            _isLocked = true;
        }

        public void Unlock ()
        {
            _isLocked = false;
        }

        public bool Interact(Interactor interactor)
        {
            if (_isOpen) Close();
            else Open(interactor.transform);
            return true;
        }

        public string GetHint()
        {
            return $"Click 'E' To {(_isOpen ? "Close" : "Open")}";
        }

        public void Open(Transform from)
        {
            if (_isOpen) return;
            if (_inProgress) return;

            if (_isLocked)
            {
                if (_audioSource) _audioSource.PlayOneShot(_lockedSound);

                if 
                (
                    !GameManager.GetMonoSystem<IDialogueMonoSystem>().IsLoaded(_onLockedDialogue) && 
                    !GameManager.GetMonoSystem<IDialogueMonoSystem>().IsLoaded(_onFirstTryLockedDialogue)
                )
                {
                    if (!_hasAttemptedToUnlock)
                    {

                        if (_onFirstTryLockedDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_onFirstTryLockedDialogue);
                        _hasAttemptedToUnlock = true;
                    }
                    else
                    {
                        if (_onLockedDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_onLockedDialogue);
                    }
                }
                return;
            }

            if (_audioSource) _audioSource.PlayOneShot(_openSound);

            _isOpen = true;
            _inProgress = true;
            float start = CurrentAngle();
            float target = ((_directionOverride == 0 && Vector3.Dot(_center.right, (_center.position - from.position).normalized) < 0) || _directionOverride < 0) ? -90 : 90;
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _openSpeed,
                (float progress) =>
                {
                    _pivot.localRotation = Quaternion.Euler(0, start + (target - start) * progress, 0);
                },
                () =>
                {
                    _inProgress = false;
                }
            );
        }

        public void Close()
        {
            if (!_isOpen) return;
            if (_inProgress) return;

            if (_audioSource) _audioSource.PlayOneShot(_closeSound);

            _inProgress = true;
            _isOpen = false;
            float start = CurrentAngle();
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _openSpeed,
                (float progress) =>
                {
                    _pivot.localRotation = Quaternion.Euler(0, start - progress * start, 0);
                },
                () =>
                {
                    _inProgress = false;
                }
            );
        }

        public void EndInteraction()
        {
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
            _hasOutline = false;
            Material[] mats = _outlineMR.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].SetInt("Boolean_8BBF99CD", 1);
            }
            _outlineMR.materials = mats;
        }

        public bool IsInteractable()
        {
            return true;
        }
    }
}
