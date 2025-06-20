using PlazmaGames.Animation;
using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;

namespace HTJ21
{
    public class MainMenuCameraController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _transitionDuration;

        [SerializeField] private Transform _cameraStart;
        [SerializeField] private GameObject _menuCar;

        public void GoToPlayer()
        {
            Vector3 startPos = transform.position;
            Quaternion startRot = transform.rotation;

            GameManager.GetMonoSystem<IAnimationMonoSystem>().RequestAnimation(
                this, 
                _transitionDuration, 
                (float t) =>
                {
                    transform.position = Vector3.Lerp(startPos, _cameraStart.position, t);
                    transform.rotation = Quaternion.Lerp(startRot, _cameraStart.rotation, t);
                },
                () =>
                {
                    GameManager.EmitEvent(new Events.StartGame());
                    gameObject.SetActive(false);
                    _menuCar.SetActive(false);
                }
            );
        }
    }
}
