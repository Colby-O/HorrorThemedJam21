using PlazmaGames.Animation;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;

namespace HTJ21
{
    public class MainMenuCameraController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private bool _skipTransition = false;

        [SerializeField] private Transform _cameraStart;
        [SerializeField] private GameObject _menuCar;
        [SerializeField] private RadioController _radio;
        [SerializeField] private AudioClip _radioTalkShowClip;

        public void Disable()
        {
            gameObject.SetActive(false);
            _menuCar.SetActive(false);
        }

        public void GoToPlayer()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().Show<GameView>();
            GameManager.GetMonoSystem<IVisibilityMonoSystem>().Load(Act.Prologue);
            GameManager.GetMonoSystem<IAudioMonoSystem>().StopAudio(PlazmaGames.Audio.AudioType.Music);

            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            if (!_skipTransition && _radioTalkShowClip) GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(_radioTalkShowClip, PlazmaGames.Audio.AudioType.Sfx, false, true);

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this,
                (!_radioTalkShowClip || _skipTransition) ? 0f : _radioTalkShowClip.length, 
                (float t) =>
                {
                    transform.position = Vector3.Lerp(startPos, _cameraStart.position, t);
                    transform.rotation = Quaternion.Lerp(startRot, _cameraStart.rotation, t);
                },
                () =>
                {
                    GameManager.EmitEvent(new Events.StartGame());
                    _radio.TurnOn();
                    _radio.NextStation();
                    Disable();
                }
            );
        }
    }
}
