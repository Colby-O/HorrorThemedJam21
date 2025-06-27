using UnityEngine;

namespace HTJ21
{
    public class Home2Director : Director
    {
        [SerializeField] private Transform _actStartLoc;
        [SerializeField] private Door _bathroomDoor;
        [SerializeField] private Shower _showerController;

        private void Setup()
        {
            if (HTJ21GameManager.Player)
            {
                HTJ21GameManager.Player.Teleport(_actStartLoc.position);
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
