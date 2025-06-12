using PlazmaGames.Console;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "SetStaticCommand", menuName = "Console Commands/Effects/SetStatic")]
    public class StaticLevelCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length == 0) msg = new ConsoleResponse("Must give a value between 0 and 1.", ResponseType.Error);
            if (float.TryParse(args[0], out float value))
            {
                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().SetStaticLevel(value);
                msg = new ConsoleResponse(ResponseType.None);
            }
            else
            {
                msg = new ConsoleResponse("Argument must be a numeric.", ResponseType.Error);
            }

            return true;
        }
    }
}
