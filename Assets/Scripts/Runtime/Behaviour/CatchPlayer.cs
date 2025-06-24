using UnityEngine;

namespace HTJ21
{
    public class CatchPlayer : MonoBehaviour
    {
        [SerializeField] private PlayerInSpotlight _seenHandler;

        private void Caught()
        {
            Debug.Log("Player Has Been Seen.");
        }

        private void Start()
        {
            if (!_seenHandler) _seenHandler = GetComponent<PlayerInSpotlight>();

            if (_seenHandler)
            {
                _seenHandler.OnPlayerHit.AddListener(Caught);
            }
        }
    }
}
