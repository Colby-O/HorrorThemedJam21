using PlazmaGames.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Splines.ExtrusionShapes;

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
        [FormerlySerializedAs("_effectLow")] [SerializeField] private float _noiseEffectLow;
        [FormerlySerializedAs("_effectHigh")] [SerializeField] private float _noiseEffectHigh;
        [FormerlySerializedAs("_effectLow")] [SerializeField] private float _chromaticEffectLow;
        [FormerlySerializedAs("_effectHigh")] [SerializeField] private float _chromaticEffectHigh;
        [SerializeField] private float _effectDistance;
        [SerializeField] private float _playerApproachSpeed;
        [SerializeField] private float _thunderInterval = 2.3f;
        [SerializeField] private float _thunderLifespan = 0.3f;
        [SerializeField] private float _fadeToBlackTime = 3;
        

        private IWeatherMonoSystem _weather;
        private float _initialThunderLifespan;

        private bool _movingMoon = false;
        private Vector3 _moonStartPosition;
        private Vector3 _moonStartScale;
        private float _moveMoveStartTime;
        private bool _captivated = false;

        private IGPSMonoSystem _gpsMs;

        public Transform GetAfterLogTarget()
        {
            return _targetAfterLog;
        }

        private Dictionary<string, DialogueSO> _dialogues = new();
        private bool _thundering = false;
        private float _lastThunder = 0;

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

        private void EndAct()
        {
            ParticleSystem.MainModule m = _weather.GetThunderHitter().main;
            m.startLifetime = new ParticleSystem.MinMaxCurve(_initialThunderLifespan);
        }

        private void Start()
        {
            _weather = GameManager.GetMonoSystem<IWeatherMonoSystem>();
            _initialThunderLifespan = _weather.GetThunderHitter().main.startLifetime.constant;
            
            LoadDialogue("FallenTree");
            LoadDialogue("HeadlightTutorial");
            LoadDialogue("DreamSighting");
            _gpsMs = GameManager.GetMonoSystem<IGPSMonoSystem>();
            GameManager.AddEventListener<Events.DreamFallenLog>(Events.NewDreamFallenLog((from, data) =>
            {
                //_gpsMs.MoveTarget(_targetAfterLog.position);
                StartDialogue("FallenTree");
            }));
            GameManager.AddEventListener<Events.DreamSighting>(Events.NewDreamSighting((from, data) =>
            {
                _weather.SpawnLightingAt(_sightingLightningHit.position);
                _sightingTree.Fall();
                _firstCultist.Walk();
                StartDialogue("DreamSighting");
            }));
            GameManager.AddEventListener<Events.DreamFoundCult>(Events.NewDreamFoundCult((from, data) =>
            {
                _movingMoon = true;
                _moonStartPosition = _moon.position;
                _moonStartScale = _moon.localScale;
                _moveMoveStartTime = Time.time;
                HTJ21GameManager.Player.LookAt(_moon);
                _captivated = true;
                HTJ21GameManager.Player.LockMoving = true;
            }));
            
            GameManager.AddEventListener<Events.DreamUnderMoon>(Events.NewDreamUnderMoon((from, data) =>
            {
                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().FadeToBlack(_fadeToBlackTime, () =>
                {
                    SceneManager.LoadScene("Act2");
                });
            }));

            GameManager.AddEventListener<Events.HeadlightTutorial>(Events.NewHeadlightTutorial((from, data) =>
            {
                StartDialogue("HeadlightTutorial");
            }));
        }

        private void Update()
        {
            if (_captivated)
            {
                float dist = Vector3.Distance(HTJ21GameManager.Player.transform.position, _cultCircle.transform.position);
                if (dist < _effectDistance)
                {
                    float val = Mathf.Lerp(_noiseEffectLow, _noiseEffectHigh, (_effectDistance - dist) / _effectDistance);
                    GameManager.GetMonoSystem<IScreenEffectMonoSystem>().SetStaticLevel(val);
                    val = Mathf.Lerp(_chromaticEffectLow, _chromaticEffectHigh, (_effectDistance - dist) / _effectDistance);
                    GameManager.GetMonoSystem<IScreenEffectMonoSystem>().SetChromicOffset(val);
                }
            }

            if (_thundering && Time.time - _lastThunder >= _thunderInterval)
            {
                _lastThunder = Time.time;
                _weather.SpawnLightingAt(_cultCircle.position);
            }
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
                    HTJ21GameManager.Player.LockMoving = false;
                    HTJ21GameManager.Player.UncontrollableApproach = _playerApproachSpeed;
                    _thundering = true;
                    ParticleSystem.MainModule m = _weather.GetThunderHitter().main;
                    m.startLifetime = new ParticleSystem.MinMaxCurve(_thunderLifespan);
                }
            }
        }
    }
}
