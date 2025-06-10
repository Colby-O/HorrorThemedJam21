using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    [System.Serializable]
    public class Dialogue
    {
        public string msg;
        public DialogueEvent dialogueEvent;
    }

    [CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/New Dialogue")]
    public class DialogueSO : ScriptableObject
    {

        [SerializeField] private List<Dialogue> _dialogues = new List<Dialogue>();
        public Queue<Dialogue> dialogues;

        public void StartDialogueEvent() => dialogues = new Queue<Dialogue>(_dialogues);
    }
}
