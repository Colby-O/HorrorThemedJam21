using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace HTJ21
{
    public class WatchdogController : MonoBehaviour
    {
        [SerializeField] private PlayerInSpotlight _seenHandler;
        [SerializeField] private Act3Director _home2Director;
        [SerializeField] private AudioClip _scareClip;
        [SerializeField] private RoadwayTracker _spotLight;

        public Act3Director GetDirector()
        {


            Director director = GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentDirector();
            if (director is Act3Director) return (Act3Director)director;
            else return null;
        }

        private void OnSeen()
        {
            if (HTJ21GameManager.Player.IsInCover()) return;

            _seenHandler.Disable();
            HTJ21GameManager.Player.LockMoving = true;
            HTJ21GameManager.Player.LookAt(transform);
            _spotLight.Stop();

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
                    _seenHandler.Enable();

                    if (_home2Director) _home2Director.ResetPlayer();
                    else GetDirector().ResetPlayer();

                    _spotLight.Continue();
                }
            );
        }

        private void Start()
        {
            _seenHandler.OnPlayerHit.AddListener(OnSeen);
        }
    }
}
