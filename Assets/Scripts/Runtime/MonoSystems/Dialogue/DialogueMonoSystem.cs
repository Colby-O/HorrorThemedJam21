using PlazmaGames.Core;
using PlazmaGames.UI;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class DialogueMonoSystem : MonoBehaviour
    {
        private bool _isDialogueInProgress = false;

        private bool _nextDialogue = false;

        private Queue<DialogueSO> _dialogueEvents;

        private DialogueSO _currentDialogueEvent = null;

        private Dialogue _currentDialogue;

        //private DialogueView _view;

        private void OpenDialogue()
        {
            _nextDialogue = false;
            _currentDialogue = _currentDialogueEvent.dialogues.Dequeue();
            //if (_currentDialogue.dialogueEvent == null) _currentDialogue.dialogueEvent = new DefaultDialogueEvent();
            //GameManager.GetMonoSystem<IUIMonoSystem>().Show<DialogueView>();
            //GameManager.GetMonoSystem<IUIMonoSystem>().GetView<DialogueView>().Display(_currentDialogue);
            _currentDialogue.dialogueEvent.OnEnter();

        }

        public void CloseDialogue()
        {
            _nextDialogue = true;
        }

        public void Load(DialogueSO dialogueEvent)
        {
            _dialogueEvents.Enqueue(dialogueEvent);
        }

        private void StartNextEvent()
        {
            if (_dialogueEvents.Count == 0)
            {
                //if (GameManager.GetMonoSystem<IUIMonoSystem>().GetCurrentViewIs<DialogueView>()) GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
                return;
            }

            if (_isDialogueInProgress == false && _currentDialogueEvent == null)
            {
                _isDialogueInProgress = true;
                _currentDialogueEvent = _dialogueEvents.Dequeue();
                _currentDialogueEvent.StartDialogueEvent();
                OpenDialogue();
            }
            else
            {
                Debug.LogWarning("Trying to load a dialogue event when there is already an event loaded!");
            }
        }

        private void DialogueUpdate()
        {
            if (_nextDialogue && _currentDialogue.dialogueEvent.CanProceed())
            {
                _currentDialogue.dialogueEvent.OnExit();
                if (_currentDialogueEvent.dialogues.Count > 0) OpenDialogue();
                else ResetDialogue();
            }
            else
            {
                _currentDialogue.dialogueEvent.OnUpdate();
            }
        }

        private void ResetDialogue()
        {
            _isDialogueInProgress = false;
            _currentDialogueEvent = null;
            _currentDialogue = null;

        }

        private void Awake()
        {
            _dialogueEvents = new Queue<DialogueSO>();
            ResetDialogue();
        }

        private void Update()
        {
            if (_isDialogueInProgress) DialogueUpdate();
            else StartNextEvent();
        }
    }
}
