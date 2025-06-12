using System.Collections.Generic;
using PlazmaGames.Core;
using TMPro;
using UnityEngine;

namespace HTJ21
{
    public class DreamDirector : MonoBehaviour
    {
        [SerializeField] private Transform _targetAfterLog;
        [SerializeField] private TreeFall _sightingTree;
        [SerializeField] private Transform _sightingLightningHit;
        [SerializeField] private WalkAndDie _firstCultist;
        
        [SerializeField] private Transform _cultCircle;
        [SerializeField] private Transform _sacrificeBody;
        [SerializeField] private Transform _moon;
        [SerializeField] private Transform _moonTarget;
        [SerializeField] private float _moonApproachSpeed = 6;
        [SerializeField] private int _moonLerpExp = 5;

        private bool _movingMoon = false;
        private Vector3 _moonStartPosition;
        private Vector3 _moonStartScale;
        private float _moveMoveStartTime;

        private IGPSMonoSystem _gpsMs;

        private Dictionary<string, DialogueSO> _dialogues = new();

        private void LoadDialogue(string name)
        {
            _dialogues.Add(name, Resources.Load<DialogueSO>($"Dialogue/Dream/{name}"));
        }

        private void StartDialogue(string name)
        {
            if (_dialogues.TryGetValue(name, out DialogueSO d))
            {
                GameManager.GetMonoSystem<IDialogueMonoSystem>().Load(d);
            }
        }
        
        private void Start()
        {
            LoadDialogue("FallenTree");
            _gpsMs = GameManager.GetMonoSystem<IGPSMonoSystem>();
            GameManager.AddEventListener<Events.DreamFallenLog>(Events.NewDreamFallenLog((from, data) =>
            {
                _gpsMs.MoveTarget(_targetAfterLog.position);
                StartDialogue("FallenTree");
            }));
            GameManager.AddEventListener<Events.DreamSighting>(Events.NewDreamSighting((from, data) =>
            {
                GameManager.GetMonoSystem<IWeatherMonoSystem>().SpawnLightingAt(_sightingLightningHit.position);
                _sightingTree.Fall();
                _firstCultist.Walk();
            }));
            GameManager.AddEventListener<Events.DreamFoundCult>(Events.NewDreamSighting((from, data) =>
            {
                _movingMoon = true;
                _moonStartPosition = _moon.position;
                _moonStartScale = _moon.localScale;
                _moveMoveStartTime = Time.time;
            }));
        }

        private void Update()
        {
            if (_movingMoon)
            {
                float t = (Time.time - _moveMoveStartTime) / _moonApproachSpeed;
                t = 1 + Mathf.Pow(t - 1, _moonLerpExp);
                _moon.transform.position = Vector3.Lerp(_moonStartPosition, _moonTarget.position, t);
                _moon.transform.localScale = Vector3.Lerp(_moonStartScale, _moonTarget.localScale, t);

                if (t >= 1)
                {
                    _movingMoon = false;
                    _moon.GetComponent<Levitate>().StartLevitateFromPosition();
                    _sacrificeBody.GetComponent<Levitate>().StopLevitatingAtNextBase();
                }
            }
        }
    }
}
