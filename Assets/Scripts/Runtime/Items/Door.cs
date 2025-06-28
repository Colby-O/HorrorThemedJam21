using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

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
        [SerializeField] private DialogueSO _onFirstOpenDialogue;
        [SerializeField, ReadOnly] private bool _hasOpenedBefore = false;
        [SerializeField, ReadOnly] private bool _hasAttemptedToUnlock = false;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;
        [SerializeField] private AudioClip _lockedSound;

        [SerializeField, ReadOnly] private bool _isOpen = false;
        [SerializeField, ReadOnly] private bool _inProgress = false;
        [SerializeField, ReadOnly] private Quaternion _startRot;

        [Header("Locked Settings")]
        [SerializeField] private bool _requiresKey = false;
        [SerializeField] private PickupableItem _key;
        [SerializeField] private bool _hasUsed = false;
        [SerializeField] private bool _isLocked = false;

        public UnityEvent OnOpen = new UnityEvent();

        public void SetDirectionOverride(int dir)
        {
            _directionOverride = dir;
        }

        private float CurrentAngle()
        {
            float angle = _pivot.localRotation.eulerAngles.y;
            angle %= 360;
            angle = angle > 180 ? angle - 360 : angle;
            return angle;
        }

        public bool IsLocked() => _isLocked;

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

        public void Open(Transform from, bool overrideAudio = false)
        {
            if (_isOpen) return;
            if (_inProgress) return;

            if (_requiresKey && !_hasUsed && HTJ21GameManager.PickupManager.HasItem(_key))
            {
                Unlock();
                _hasUsed = true;
            }

            if (_isLocked)
            {
                if (!overrideAudio && _audioSource && _lockedSound) _audioSource.PlayOneShot(_lockedSound);

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

            if (!_hasOpenedBefore)
            {
                if (_onFirstOpenDialogue) GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(_onFirstOpenDialogue);
                _hasOpenedBefore = true;
            }

            if (!overrideAudio && _audioSource && _openSound) _audioSource.PlayOneShot(_openSound);
            OnOpen.Invoke();

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

        public void Restart()
        {
            Close(true, true);
            _hasAttemptedToUnlock = false;
            _hasOpenedBefore = false;
            _hasUsed = false;
        }

        public void Close(bool overrideAudio = false, bool force = false)
        {
            if (force)
            {
                GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
                _isOpen = false;
                _inProgress = false;
                _pivot.localRotation = _startRot;
                if (!overrideAudio && _audioSource && _closeSound) _audioSource.PlayOneShot(_closeSound);
                return;
            }

            if (!_isOpen) return;
            if (_inProgress) return;

            if (!overrideAudio && _audioSource && _closeSound) _audioSource.PlayOneShot(_closeSound);

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

        private void Awake()
        {
            _hasOpenedBefore = false;
            _hasAttemptedToUnlock = false;
            _startRot = _pivot.localRotation;
        }

        private void OnDisable()
        {
            GameManager.GetMonoSystem<IAnimationMonoSystem>().StopAllAnimations(this);
            _pivot.localRotation = Quaternion.Euler(0, 0, 0);
            _inProgress = false;
            _isOpen = false;
        }
    }
}
