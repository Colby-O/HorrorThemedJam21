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
        [SerializeField] private WalkAndDie _firstCultist;

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
        
        void Start()
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
                _sightingTree.Fall();
                _firstCultist.Walk();
            }));
        }
    }
}
