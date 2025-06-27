using PlazmaGames.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class Act2Director : Director
    {
        [SerializeField] private Transform _actStartLoc;

        [Header("Shower")]
        [SerializeField] private Shower _showerController;

        private void OnShowerFinished()
        {
            _showerController.RestoreToDefaults();
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        private void Setup()
        {
            if (HTJ21GameManager.Player)
            {
                HTJ21GameManager.Player.Teleport(_actStartLoc.position);
            }

            _showerController.OnShowerFinish.AddListener(OnShowerFinished);
        }

        private void AddEvents()
        {

        }

        public override void OnActEnd()
        {
            _showerController.OnShowerFinish.RemoveListener(OnShowerFinished);
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
