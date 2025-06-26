using PlazmaGames.Attribute;
using PlazmaGames.Core;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class Home2Director : Director
    {
        [SerializeField] private PlayerInSpotlight _moonController;

        [Header("Sections")]
        [SerializeField] private GameObject _roadSection;
        [SerializeField] private GameObject _roomSection;
        [SerializeField] private GameObject _showcaseSection;

        [SerializeField] private GameObject _moonBeam;

        [Header("References")]
        [SerializeField] private Door _enterRoomDoor;
        [SerializeField] private Door _exitRoomDoor;

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
        }

        private void AddEvents()
        {
            GameManager.AddEventListener<Events.VoidNextCheck>(Events.NewVoidNextCheck((from, data) =>
            {
                NextCheckpoint();
            }));

            GameManager.AddEventListener<Events.RoadSectionFinished>(Events.NewRoadSectionFinished((from, data) =>
            {
                _roomSection.SetActive(true);
            }));


            GameManager.AddEventListener<Events.RoomSectionStart>(Events.NewRoomSectionStart((from, data) =>
            {
                _moonBeam.SetActive(false);
                _enterRoomDoor.Close();
                _enterRoomDoor.Lock();
                NextCheckpoint();
            }));

            GameManager.AddEventListener<Events.RoomSectionFinished>(Events.NewRoomSectionFinished((from, data) =>
            {
                _roadSection.SetActive(false);
                _roomSection.SetActive(false);
                _showcaseSection.SetActive(true);
                _exitRoomDoor.Close();
                _exitRoomDoor.Lock();
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
