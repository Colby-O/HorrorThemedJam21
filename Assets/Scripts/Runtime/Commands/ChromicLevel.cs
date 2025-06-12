using PlazmaGames.Console;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "SetChromicOffsetCommand", menuName = "Console Commands/Effects/SetChromicOffset")]
    public class ChromicLevel : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length == 0)
            {
                msg = new ConsoleResponse("Must give a value between 0 and 1.", ResponseType.Error);
                return false;
            }
            if (float.TryParse(args[0], out float value))
            {
                GameManager.GetMonoSystem<IScreenEffectMonoSystem>().SetChromicOffset(value);
                msg = new ConsoleResponse(ResponseType.None);
                return true;
            }
            else
            {
                msg = new ConsoleResponse("Argument must be a numeric.", ResponseType.Error);
            }

            return false;
        }
    }
}
