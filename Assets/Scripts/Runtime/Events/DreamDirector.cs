using PlazmaGames.Core;
using TMPro;
using UnityEngine;

namespace HTJ21
{
    public class DreamDirector : MonoBehaviour
    {
        [SerializeField] private Transform _targetAfterLog;

        private IGPSMonoSystem _gpsMs;
        
        void Start()
        {
            _gpsMs = GameManager.GetMonoSystem<IGPSMonoSystem>();
            GameManager.AddEventListener<Events.DreamFallenLog>(Events.NewDreamFallenLog((from, data) =>
            {
                _gpsMs.MoveTarget(_targetAfterLog.position);
            }));
            GameManager.AddEventListener<Events.DreamSighting>(Events.NewDreamSighting((from, data) =>
            {
                
            }));
        }
    }
}
