using PlazmaGames.Animation;
using PlazmaGames.Attribute;
using PlazmaGames.Core;
using Unity.VisualScripting;
using UnityEngine;

namespace HTJ21
{
    public class Shower : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool _isSpookyShower = false;
        [SerializeField] ParticleSystem _waterPS;
        [SerializeField] private GameObject _openCurtain;
        [SerializeField] private GameObject _closeCurtain;
        [SerializeField] private MeshRenderer _waterBed;
        [SerializeField] private GameObject _blood;
        [SerializeField] private float _showerDuration;
        [SerializeField] private float _showerFadeoutDuration;
        [SerializeField, ReadOnly] private bool _isShowering = false;

        [Header("Audio")]
        [SerializeField] private AudioSource _as;
        [SerializeField] private AudioClip _showerClip;

        [Header("Outline")]
        [SerializeField] private MeshRenderer _outlineMR;
        [SerializeField, ReadOnly] private bool _hasOutline = false;

        public void EndInteraction()
        {

        }

        public string GetHint()
        {
            return $"Click 'E' to {((_isSpookyShower) ? "cleanse yourself" : "shower")}";
        }

        public bool Interact(Interactor interactor)
        {
            if (!_isSpookyShower) StartShower();
            else SpookyShower();
            return true;
        }

        public bool IsInteractable()
        {
            return !_isShowering;
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

        private void RestoreToDefaults(bool keepBlood = false)
        {
            _isShowering = false;
            _waterPS.gameObject.SetActive(false);
            _closeCurtain.SetActive(true);
            _openCurtain.SetActive(false);
            _waterBed.gameObject.SetActive(false);
            if (!keepBlood) _blood.gameObject.SetActive(false);
        }

        private void SpookyShower()
        {
            if (_as) _as.Play();

            _isShowering = true;
            _waterPS.gameObject.SetActive(true);

            _closeCurtain.SetActive(false);
            _openCurtain.SetActive(true);
            _waterBed.gameObject.SetActive(true);
            _blood.gameObject.SetActive(true);

            _waterBed.material.color = Color.red;

            ParticleSystem.MainModule main = _waterPS.main;
            main.startColor = Color.red;

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _showerDuration,
                (float t) =>
                {
                },
                () =>
                {
                    GameManager.GetMonoSystem<IScreenEffectMonoSystem>().FadeToBlack(
                        _showerFadeoutDuration,
                        () =>
                        {
                            if (_as) _as.Stop();
                            RestoreToDefaults(true);
                            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().RestoreDefaults();
                            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
                        }
                    );
                }
            );
        }

        private void StartShower()
        {
            if (_as) _as.Play();

            _isShowering = true;
            _waterPS.gameObject.SetActive(true);

            _closeCurtain.SetActive(false);
            _openCurtain.SetActive(true);
            _waterBed.gameObject.SetActive(true);

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this, 
                _showerDuration, 
                (float t) => 
                { 
                }, 
                () => 
                {
                    GameManager.GetMonoSystem<IScreenEffectMonoSystem>().FadeToBlack(
                        _showerFadeoutDuration, 
                        () => 
                        {
                            if (_as) _as.Stop();
                            RestoreToDefaults();
                            GameManager.GetMonoSystem<IScreenEffectMonoSystem>().RestoreDefaults();
                            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
                        }
                    );
                }
            );
        }

        private void Awake()
        {
            RestoreToDefaults();

            if (!_as) _as = GetComponent<AudioSource>();

            if (_as)
            {
                _as.clip = _showerClip;
                _as.loop = true;
                _as.Stop();
            }
        }
    }
}
