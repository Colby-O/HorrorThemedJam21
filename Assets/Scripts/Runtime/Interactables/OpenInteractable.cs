using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class OpenInteractable : MonoBehaviour, IInteractable
    {
        [Header("Translation")]
        [SerializeField] private bool _canTranslate = true;
        [SerializeField] private Vector3 _endPos;

        [Header("Rotation")]
        [SerializeField] private bool _canRotate;
        [SerializeField] private Vector3 _endRot;

        [Header("Settings")]
        [SerializeField] private float _openTime = 1f;

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

        [Header("Audio")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _openSound;
        [SerializeField] private AudioClip _closeSound;

        [SerializeField, ReadOnly] private Vector3 _startPos;
        [SerializeField, ReadOnly] private Vector3 _startRot;

        [SerializeField, ReadOnly] private bool _isOpen = false;
        [SerializeField, InspectorButton("Open")] private bool _open = false;
        [SerializeField, InspectorButton("Close")] private bool _close = false;

        void OpenStep(float t, Vector3 startRot, Vector3 endRot, Vector3 startPos, Vector3 endPos)
        {
            if (_canRotate)
            {
                Quaternion start = Quaternion.Euler(startRot);
                Quaternion end = Quaternion.Euler(endRot);
                transform.localRotation = Quaternion.Lerp(start, end, t);
            }
            if (_canTranslate)
            {
                transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            }
        }

        public void Open()
        {
            if (_isOpen) return;
            if (_audioSource) _audioSource.PlayOneShot(_openSound);
            _isOpen = true;
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(this, _openTime, (float t) => OpenStep(t, _startRot, _endRot, _startPos, _endPos));
        }

        public void Close()
        {
            if (!_isOpen) return;
            if (_audioSource) _audioSource.PlayOneShot(_closeSound);
            _isOpen = false;
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(this, _openTime, (float t) => OpenStep(t, _endRot, _startRot, _endPos, _startPos));
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

        public bool Interact(Interactor interactor)
        {
            if (_isOpen) Close();
            else Open();
            return true;
        }

        public void EndInteraction()
        {

        }

        public string GetHint()
        {
            return $"Click 'E' To {(_isOpen ? "Close" : "Open")}";
        }

        private void Awake()
        {
            _startPos = transform.localPosition;
            _startRot = transform.localRotation.eulerAngles;
        }
    }
}
