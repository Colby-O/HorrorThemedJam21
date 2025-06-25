using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class WatchdogController : MonoBehaviour
    {
        [SerializeField] private PlayerInSpotlight _seenHandler;

        public Home2Director GetDirector()
        {
            Director director = GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentDirector();
            if (director is Home2Director) return (Home2Director)director;
            else return null;
        }

        private void OnSeen()
        {
            GetDirector().ResetPlayer();
        }

        private void Awake()
        {
            _seenHandler.OnPlayerHit.AddListener(OnSeen);
        }
    }
}
