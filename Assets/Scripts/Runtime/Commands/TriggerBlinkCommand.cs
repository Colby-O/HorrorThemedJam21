using PlazmaGames.Console;
using PlazmaGames.Core;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "TriggerBlinkCommand", menuName = "Console Commands/Effects/TriggerBlink")]
    public class TriggerBlinkCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length <= 1)
            {
                msg = new ConsoleResponse("Must give a duration and number of blinks.", ResponseType.Error);
                return false;
            }
            if (float.TryParse(args[0], out float duration))
            {
                if (int.TryParse(args[1], out int num))
                {

                    GameManager.GetMonoSystem<IScreenEffectMonoSystem>().TriggerBlink(duration, num, null);
                    msg = new ConsoleResponse(ResponseType.None);
                    return true;
                }
                else
                {
                    msg = new ConsoleResponse("Second argument must be a integer.", ResponseType.Error);

                }
            }
            else
            {
                msg = new ConsoleResponse("First argument must be a numeric.", ResponseType.Error);
            }

            return false;
        }
    }
}
