using UnityEngine;

namespace HTJ21
{
    public class Home2Director : Director
    {
        [SerializeField] private MoonController _moonController;
        [SerializeField] private Transform _startLoc;

        private void ResetPlayer()
        {
            if (HTJ21GameManager.Player)
            {
                HTJ21GameManager.Player.GetComponent<CharacterController>().enabled = false;
                HTJ21GameManager.Player.transform.position = _startLoc.position;
                HTJ21GameManager.Player.GetComponent<CharacterController>().enabled = true;
            }
        }

        public override void OnActEnd()
        {

        }

        public override void OnActInit()
        {
            _moonController.OnPlayerHit.AddListener(ResetPlayer);
        }

        public override void OnActStart()
        {

        }

        public override void OnActUpdate()
        {

        }
    }
}
