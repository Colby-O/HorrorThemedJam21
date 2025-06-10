using UnityEngine;

namespace HTJ21
{
    public interface IDialogueMonoSystem 
    {
        public void CloseDialogue();
        public void Load(DialogueSO dialogueEvent);
    }
}
