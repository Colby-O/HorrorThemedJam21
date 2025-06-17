using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    public class Act1Director : MonoBehaviour
    {
        [SerializeField] private Transform _gpsTarget;
        [SerializeField] private Transform _gpsTargetReroute1;
        [SerializeField] private Transform _gpsTargetReroute2;
        [SerializeField] private Transform _gpsTargetHome;
        [SerializeField] private TreeFall _tree;
        [SerializeField] private WalkAndDie _cultistAtTree;
        
        void Start()
        {
            GameManager.AddEventListener<Events.TreeFall>(Events.NewTreeFall((from, data) =>
            {
                _cultistAtTree.Walk();
                _tree.Fall();
                _gpsTarget.position = _gpsTargetReroute1.position;
            }));
            GameManager.AddEventListener<Events.TreeReroute1>(Events.NewTreeFall((from, data) =>
            {
                _gpsTarget.position = _gpsTargetReroute2.position;
            }));
            GameManager.AddEventListener<Events.TreeReroute2>(Events.NewTreeFall((from, data) =>
            {
                _gpsTarget.position = _gpsTargetHome.position;
            }));
        }
    }
}
