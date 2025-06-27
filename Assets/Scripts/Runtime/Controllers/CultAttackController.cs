using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class CultAttackController : MonoBehaviour
    {
        [SerializeField] private PlayerInSpotlight _seenHandler;
        [SerializeField] private MoveAlongSpline _alongSpline;
        
        [SerializeField] private Light _scareLight;
        [SerializeField] private float _moveDuration;
        [SerializeField] private Transform _scareLoc;
        [SerializeField] private Transform _playerDuringLoc;
        [SerializeField] private AudioClip _scareClip;

        [SerializeField] private Act3Director _home2Director;

        public Act3Director GetDirector()
        {
            Director director = GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentDirector();
            if (director is Act3Director) return (Act3Director)director;
            else return null;
        }

        private void OnSeen()
        {
            if (_home2Director) _home2Director.ResetPlayer();
            else GetDirector().ResetPlayer();
        }

        private void OnCaught()
        {
            _alongSpline.Stop();
            _seenHandler.Disable();
            _scareLight.gameObject.SetActive(true);

            HTJ21GameManager.Player.LockMoving = true;
            HTJ21GameManager.Player.LookAt(_scareLoc);

            Vector3 start = HTJ21GameManager.Player.transform.position;
            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                _moveDuration,
                (float t) =>
                {
                    HTJ21GameManager.Player.transform.position = Vector3.Lerp(start, _playerDuringLoc.position, t);
                },
                () => 
                {
                    GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_scareClip, PlazmaGames.Audio.AudioType.Sfx, false, true);
                    GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                        this,
                        _scareClip.length,
                        (float t) =>
                        {
                        },
                        () =>
                        {
                            HTJ21GameManager.Player.LockMoving = false;
                            HTJ21GameManager.Player.StopLookAt();
                            _alongSpline.Continue();
                            _seenHandler.Enable();
                            _scareLight.gameObject.SetActive(false);
                            OnSeen();
                        }
                    );
                }   
            );
        }

        private void Start()
        {
            _scareLight.gameObject.SetActive(false);

            if (!_seenHandler) _seenHandler = GetComponent<PlayerInSpotlight>();

            if (_seenHandler)
            {
                _seenHandler.OnPlayerHit.AddListener(OnCaught);
            }
        }
    }
}
