using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    public class ShowInRange : MonoBehaviour
    {
        [SerializeField] private GameObject _obj;
        [SerializeField] private float _dstMax;
        [SerializeField] private float _dstMin;
        [SerializeField] private bool _keepShownOnceActive = false;

        [Header("Audio")]
        [SerializeField] private AudioSource _as;
        [SerializeField] private AudioClip _onShowClip;

        [SerializeField, ReadOnly] private bool _hasShown = false;

        public void Restart()
        {
            _hasShown = false;
        }

        private void Awake()
        {
            _hasShown = false;
        }

        private void Update()
        {
            if (_keepShownOnceActive && _hasShown) return;

            if (!_obj || !HTJ21GameManager.CurrentControllable || HTJ21GameManager.IsPaused) return;
            float dst = Vector3.Distance(HTJ21GameManager.CurrentControllable.transform.position, transform.position);
            _obj.gameObject.SetActive(dst < _dstMax && dst > _dstMin);

            if (!_hasShown && _obj.activeSelf)
            {
                _hasShown = true;
                if (_as && _onShowClip) _as.PlayOneShot(_onShowClip);
            }
        }
    }
}
