using PlazmaGames.Console;
using PlazmaGames.Core;
using System;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "LoadActCommand", menuName = "Console Commands/Act/Load")]
    public class LoadActCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length == 0)
            {
                msg = new($"You must pass a act name.", ResponseType.Warning);
                return false;
            }

            if (Enum.TryParse<Act>(args[0], out Act act))
            {
                msg = new($"Loading {act}.", ResponseType.Response);
                GameManager.GetMonoSystem<IDirectorMonoSystem>().StartAct(act);
            }
            else
            {
                msg = new($"You must pass a vaild act name. {args[0]} is not an act.", ResponseType.Warning);
                return false;
            }

            return true;
        }
    }
}
