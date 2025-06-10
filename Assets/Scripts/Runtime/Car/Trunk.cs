using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections;
using UnityEngine;

namespace HTJ21
{
    public class Trunk : MonoBehaviour, IInteractable
    {
        [SerializeField] private Vector3 _endRot;
        [SerializeField] private float _openTime = 1f;
        [SerializeField] private GameObject _outline;
        [SerializeField, ReadOnly] private Vector3 _startRot;
        [SerializeField, ReadOnly] private bool _isOpen = false;
        [SerializeField, InspectorButton("Open")] private bool _open = false;
        [SerializeField, InspectorButton("Close")] private bool _close = false;

        void OpenStep(float t, Vector3 startRot, Vector3 endRot)
        {
            Quaternion start = Quaternion.Euler(startRot);
            Quaternion end = Quaternion.Euler(endRot);

            transform.localRotation = Quaternion.Lerp(start, end, t);
        }

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(this, _openTime, (float t) => OpenStep(t, _startRot, _endRot));
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(this, _openTime, (float t) => OpenStep(t, _endRot, _startRot));
        }

        public void AddOutline()
        {
            _outline.SetActive(true);
        }

        public void RemoveOutline()
        {
            _outline.SetActive(false);
        }

        public bool IsInteractable()
        {
            return true;
        }

        public void OnPickup(Interactor interactor)
        {

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
            return string.Empty;
        }

        private void Awake()
        {
            _startRot = transform.localRotation.eulerAngles;
            RemoveOutline();
        }
        private void LateUpdate()
        {
            RemoveOutline();
        }
    }
}
