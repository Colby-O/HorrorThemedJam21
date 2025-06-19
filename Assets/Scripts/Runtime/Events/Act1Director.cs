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

        private IGPSMonoSystem _gpsMs;
        
        private void Start()
        {
            _gpsMs = GameManager.GetMonoSystem<IGPSMonoSystem>();
            GameManager.AddEventListener<Events.TreeFall>(Events.NewTreeFall((from, data) =>
            {
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
            GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOn();
        }

        public override void OnActUpdate()
        {
        }

        public override void OnActEnd()
        {
        }
    }
}
