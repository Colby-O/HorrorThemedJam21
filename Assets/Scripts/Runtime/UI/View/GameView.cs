using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace HTJ21
{
    public class GameView : View
    {
        [Header("Reference")]
        [SerializeField] private AudioSource _as;
        [SerializeField] private GameObject _holder;

        [Header("Settings")]
        [SerializeField] private float _typeSpeed = 0.1f;
        [SerializeField] private float _timeout = 4f;
        [SerializeField] private float _timeoutShow = 1f;

        [Header("Hints")]
        [SerializeField] private TMP_Text _hint;

        [Header("Dialogue")]
        [SerializeField] private float _waitDelayDialogue = 1f;
        [SerializeField] private TMP_Text _dialogueAvatarName;
        [SerializeField] private TMP_Text _dialogue;

        [Header("Location Display")]
        [SerializeField] private float _waitDelayLocation = 2f;
        [SerializeField] private TMP_Text _location;
        private UnityAction _currentLocationCallback = null;

        [SerializeField, ReadOnly] private bool _isWriting = false;
        [SerializeField, ReadOnly] private bool _isWritingDialogue = false;
        [SerializeField, ReadOnly] private bool _isWritingLocation = false;
        [SerializeField, ReadOnly] private float _timeSinceWriteStart = 0f;
        [SerializeField, ReadOnly] private string _currentMessage;
        [SerializeField, ReadOnly] private bool _showedMessage = false;

        IEnumerator TypeDialogue(string msg, float delay, float typeSpeed, TMP_Text target, bool isDialogue, UnityAction onFinished = null)
        {
            _isWriting = true;
            _currentMessage = msg;
            _showedMessage = false;

            if (isDialogue) _isWritingDialogue = true;
            else _isWritingLocation = true;

            _timeSinceWriteStart = 0f;

            _as?.Play();

            target.text = string.Empty;
            foreach (char c in msg)
            {
                while (HTJ21GameManager.IsPaused) yield return null;

                target.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }

            _as?.Stop();

            _showedMessage = true;
            yield return new WaitForSeconds(delay);

            if (isDialogue) _isWritingDialogue = false;
            else _isWritingLocation = false;

            _currentMessage = string.Empty;
            _showedMessage = false;

            if (onFinished != null) onFinished.Invoke();
        }

        private void Next()
        {
            _isWriting = false;
            GameManager.GetMonoSystem<IDialogueMonoSystem>().CloseDialogue();
        }

        public void SetHint(string hint)
        {
            _hint.text = hint;
            _hint.transform.parent.gameObject.SetActive(true);
        }

        public void HideHint()
        {
            _hint.text = string.Empty;
            _hint.transform.parent.gameObject.SetActive(false);
        }

        public void ShowDialogue()
        {
            _dialogue.transform.parent.gameObject.SetActive(true);
        }

        public void DisplayDialogue(Dialogue dialogue)
        {
            _dialogueAvatarName.text = dialogue.avatarName;
            StartCoroutine(TypeDialogue(
                dialogue.msg[HTJ21GameManager.Preferences.SelectedLanguage], 
                (dialogue.waitDelayOverride <= 0) ? _waitDelayDialogue : dialogue.waitDelayOverride, 
                (dialogue.typeSpeedOverride <= 0) ? _typeSpeed : dialogue.typeSpeedOverride,
                _dialogue, 
                true, 
                Next)
            );
        }

        public void HideDialogue()
        {
            _dialogueAvatarName.text = string.Empty;
            _dialogue.text = string.Empty;
            _dialogue.transform.parent.gameObject.SetActive(false);
        }

        public void SkipDialogue()
        {
            if(_isWritingDialogue)
            {
                StopAllCoroutines();
                _isWriting = false;
                _showedMessage = false;
                _isWritingDialogue = false;
                _isWritingLocation = false;
                _dialogue.text = string.Empty;
                _as?.Stop();
                Next();
            }
        }


        public void SkipLocation()
        {
            if (_isWritingLocation)
            {
                StopAllCoroutines();
                HideLocation();
            }
        }

        public void ShowLocation(string location, UnityAction onFinished = null)
        {
            _location.transform.parent.gameObject.SetActive(true);
            _currentLocationCallback = onFinished;
            StartCoroutine(TypeDialogue(location, _waitDelayLocation, _typeSpeed, _location, false, () => { if (onFinished != null) onFinished.Invoke(); HideLocation(); }));
        }

        public void HideLocation()
        {
            _currentLocationCallback = null;
            _location.transform.parent.gameObject.SetActive(false);
        }

        public override void Init()
        {
            HideHint();
            HideDialogue();
            HideLocation();
        }

        public override void Show()
        {
            base.Show();
            _holder.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void HandleTimeout()
        {
            if (HTJ21GameManager.IsPaused) return;

            if (_isWriting) _timeSinceWriteStart += Time.deltaTime;

            if (_timeSinceWriteStart > _timeout && !_showedMessage)
            {
                _showedMessage = true;
                if (_isWritingDialogue) _dialogue.text = _currentMessage;
                else _location.text = _currentMessage;
            }
            else if (_timeSinceWriteStart > _timeout + _timeoutShow)
            {
                _isWriting = false;

                if (_isWritingDialogue)
                {
                    Next();
                }
                else
                {
                    if (_currentLocationCallback != null) _currentLocationCallback.Invoke();
                    HideLocation();
                }

                _showedMessage = false;
                _isWritingDialogue = false;
                _isWritingLocation = false;
                _as?.Stop();
            }
        }

        public override void Hide()
        {
            _holder.SetActive(false);
        }

        private void Start()
        {
            GameManager.GetMonoSystem<IInputMonoSystem>().SkipCallback.AddListener(SkipDialogue);
        }

        private void Update()
        {
            HideHint();
            HandleTimeout();
        }
    }
}
