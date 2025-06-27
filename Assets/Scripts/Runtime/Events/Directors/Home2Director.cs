using PlazmaGames.Core;
using PlazmaGames.Core.Debugging;
using UnityEngine;

namespace HTJ21
{
    public class Home2Director : Director
    {
        [SerializeField] private Transform _actStartLoc;
        [SerializeField] private Door _bathroomDoor;
        [SerializeField] private Shower _showerController;

        private void OnPortalEnter(Portal p1, Portal p2)
        {
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.RemoveListener(OnPortalEnter);
            p1.gameObject.SetActive(false);
            p2.gameObject.SetActive(false);
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        private void Setup()
        {
            if (HTJ21GameManager.Player)
            {
                HTJ21GameManager.Player.Teleport(_actStartLoc.position);
                HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.AddListener(OnPortalEnter);
                PlazmaDebug.LogError("Playing was not found.", "Home 2 Director", 1, Color.red);
            }

            _bathroomDoor.Close();
            _bathroomDoor.Lock();

            _showerController.StartShower(false);
            _showerController.Disable();
        }

        private void AddEvents()
        {

        }

        public override void OnActEnd()
        {

        }

        public override void OnActInit()
        {

        }

        public override void OnActStart()
        {
            Setup();
            AddEvents();
        }

        public override void OnActUpdate()
        {


        }
    }
}
