using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    public class LampController : MonoBehaviour, IInteractable
    {
        [SerializeField] private Light _lampLight;

        [Header("Audio")]
        [SerializeField] private AudioSource _as;
        [SerializeField] private AudioClip _onClip;
        [SerializeField] private AudioClip _offClip;

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

        public bool IsOn()
        {
            return _lampLight.gameObject.activeSelf;
        }

        public void TurnOn(bool overrideAudio = false)
        {
            if (_as && !overrideAudio) _as.PlayOneShot(_onClip);
            _lampLight.gameObject.SetActive(true);
        }

        public void TurnOff(bool overrideAudio = false)
        {
            if (_as && !overrideAudio) _as.PlayOneShot(_offClip);
            _lampLight.gameObject.SetActive(false);
        }

        private void Awake()
        {
            TurnOn(true);
            if (!_as) _as = GetComponent<AudioSource>();
        }

        public bool IsInteractable()
        {
            return true;
        }

        public bool Interact(Interactor interactor)
        {
            if (_lampLight.gameObject.activeSelf) TurnOff();
            else TurnOn();
            return true;
        }

        public void EndInteraction()
        {

        }

        public string GetHint()
        {
            return $"Click 'E' to turn {(_lampLight.gameObject.activeSelf ? "off" : "on")}";
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
    }
}
