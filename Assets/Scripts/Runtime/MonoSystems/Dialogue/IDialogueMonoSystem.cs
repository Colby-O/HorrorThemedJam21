using PlazmaGames.Core.MonoSystem;
using UnityEngine;

namespace HTJ21
{
    public interface IDialogueMonoSystem : IMonoSystem
    {
        public void CloseDialogue();
        public void Load(DialogueSO dialogueEvent);
        public void ResetDialogue();
    }
}
