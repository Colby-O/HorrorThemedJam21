using UnityEngine;

namespace HTJ21
{
    public class Chest : MonoBehaviour, IInteractable
    {
        [SerializeField] private MeshRenderer[] _outlineMR;
        [SerializeField] private float _openAngle = -70;
        [SerializeField] private float _openSpeed = 2;

        [SerializeField] private AudioSource _as;
        [SerializeField] private AudioClip _openClip;
        [SerializeField] private AudioClip _closeClip;

        
        private bool _hasOutline = false;
        private bool _open = false;
        private float _angle = 0;

        private Transform _pivot;

        private void Start()
        {
            _pivot = transform.parent;
        }

        private void Update()
        {
            float targetAngle = _open ? _openAngle : 0;
            float dir = Mathf.Sign(targetAngle - _angle);
            float nextAngle = _angle + _openSpeed * dir * Time.deltaTime;
            if (Mathf.RoundToInt(Mathf.Sign(targetAngle - nextAngle)) != (int)dir)
            {
                nextAngle = targetAngle;
            }
            _angle = nextAngle;
            _pivot.localEulerAngles = _pivot.localEulerAngles.SetZ(_angle);
        }

        public void AddOutline()
        {
            _hasOutline = true;
            foreach (MeshRenderer mr in _outlineMR)
            {
                Material[] mats = mr.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].SetInt("Boolean_8BBF99CD", 0);
                }

                mr.materials = mats;
            }
        }

        public void RemoveOutline()
        {
            _hasOutline = false;
            foreach (MeshRenderer mr in _outlineMR)
            {
                Material[] mats = mr.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].SetInt("Boolean_8BBF99CD", 1);
                }

                mr.materials = mats;
            }
        }

        public bool IsInteractable()
        {
            return true;
        }

        public bool Interact(Interactor interactor)
        {
            _open = !_open;

            if (_open && _as && _openClip) _as.PlayOneShot(_openClip);
            else if (!_open && _as && _closeClip) _as.PlayOneShot(_closeClip);

            return true;
        }

        public void EndInteraction()
        {
        }

        public string GetHint()
        {
            return "Click 'E' To Open";
        }

        public void Restart()
        {
            _open = false;
            _pivot.localEulerAngles = _pivot.localEulerAngles.SetZ(0f);
        }
    }
}
