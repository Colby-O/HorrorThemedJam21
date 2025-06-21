using System.Collections.Generic;
using System.Linq;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class Act1Director : Director
    {
        [SerializeField] private Transform _gpsTarget;
        [SerializeField] private Transform _gpsTargetReroute1;
        [SerializeField] private Transform _gpsTargetReroute2;
        [SerializeField] private Transform _gpsTargetHome;
        [SerializeField] private TreeFall _tree;
        [SerializeField] private WalkAndDie _cultistAtTree;

        [SerializeField] private Door _murderHouseDoor;
        [SerializeField] private GameObject _murderHousePortal;
        [SerializeField] private GameObject _murderHouseReturnPortal;
        
        [SerializeField] private GameObject _murderHouseDoorBoards;

        private List<Rigidbody> _doorBoards;

        private IGPSMonoSystem _gpsMs;
        
        public override void OnActInit()
        {
            _murderHousePortal.SetActive(false);
            _murderHouseReturnPortal.SetActive(false);
            _gpsMs = GameManager.GetMonoSystem<IGPSMonoSystem>();

            _doorBoards = _murderHouseDoorBoards.GetComponentsInChildren<Rigidbody>().ToList();
                
            GameManager.AddEventListener<Events.TreeFall>(Events.NewTreeFall((from, data) =>
            {
                GameManager.GetMonoSystem<IWeatherMonoSystem>().SpawnLightingAt(_tree.transform.position);
                _cultistAtTree.Walk();
                _tree.Fall();
                _gpsMs.MoveTarget(_gpsTargetReroute1.position);
            }));
            GameManager.AddEventListener<Events.TreeReroute1>(Events.NewTreeFall((from, data) =>
            {
                _gpsMs.MoveTarget(_gpsTargetReroute2.position);
            }));
            GameManager.AddEventListener<Events.TreeReroute2>(Events.NewTreeFall((from, data) =>
            {
                _gpsMs.MoveTarget(_gpsTargetHome.position);
            }));
        }

        public override void OnActStart()
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player) player.EnablePlayer();
            HTJ21GameManager.IsPaused = false;
            HTJ21GameManager.CinematicCar?.Disable();
            HTJ21GameManager.CarTutorial.ShowTutorial(0);
            GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOn();
            _murderHouseDoor.Lock();
            _murderHouseDoor.OnOpen.AddListener(() =>
            {
                _murderHousePortal.SetActive(true);
                _murderHouseReturnPortal.SetActive(true);
                HTJ21GameManager.HouseController.EnableHouse();
            });
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.AddListener(OnPortalEnter);
        }
        
        private void OnPortalEnter(Portal p1, Portal p2)
        {
            HTJ21GameManager.Player.GetComponent<PortalObject>().OnPortalEnter.RemoveListener(OnPortalEnter);
            p2.gameObject.SetActive(false);
            p1.gameObject.SetActive(false);
            GameManager.GetMonoSystem<IDirectorMonoSystem>().NextAct();
        }

        public override void OnActUpdate()
        {
            if (_doorBoards.All(e => e.isKinematic == false))
            {
                _murderHouseDoor.Unlock();
            }
        }

        public override void OnActEnd()
        {
        }
    }
}
