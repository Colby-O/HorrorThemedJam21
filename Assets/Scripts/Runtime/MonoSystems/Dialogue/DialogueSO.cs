using System.Collections.Generic;
using UnityEngine;
using PlazmaGames.Runtime.DataStructures;

namespace HTJ21
{
    [System.Serializable]
    public enum Language
    {
        EN,
        FR
    }

    [System.Serializable]
    public class Dialogue
    {
        public string avatarName;
        public SerializableDictionary<Language, string> msg;
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
