using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class Home2Director : Director
    {
        [SerializeField] private PlayerInSpotlight _moonController;

        [Header("Checkpoints")]
        [SerializeField] private List<Transform> _checkpoints;

        [SerializeField, ReadOnly] private int _currentCheckpoint;

        public void ResetPlayer()
        {
            if (_currentCheckpoint >= _checkpoints.Count || _currentCheckpoint < 0) return;

            if (HTJ21GameManager.Player)
            {
                HTJ21GameManager.Player.GetComponent<CharacterController>().enabled = false;
                HTJ21GameManager.Player.transform.position = _checkpoints[_currentCheckpoint].position;
                HTJ21GameManager.Player.GetComponent<CharacterController>().enabled = true;
            }
        }

        public void NextCheckpoint()
        {
            _currentCheckpoint++;
        }

        private void Setup()
        {
            _currentCheckpoint = 0;
            _moonController.OnPlayerHit.AddListener(ResetPlayer);
        }

        private void AddEvents()
        {
            GameManager.AddEventListener<Events.VoidNextCheck>(Events.NewVoidNextCheck((from, data) =>
            {
                NextCheckpoint();
            }));
        }

        public override void OnActEnd()
        {

        }

        public override void OnActInit()
        {
            Setup();
            AddEvents();
        }

        public override void OnActStart()
        {
            //Setup();
            //AddEvents();
        }

        public override void OnActUpdate()
        {

        }
    }
}
