using PlazmaGames.Console;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "ToggleGPSCommand", menuName = "Console Commands/Car/GPSToggle")]
    public class ToggleGPSCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length == 0)
            {
                msg = new ConsoleResponse("Must give 0 and 1.", ResponseType.Error);
                return false;
            }
            if (int.TryParse(args[0], out int value))
            {
                if (GameManager.Instance)
                {
                    if (value == 0) GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOff();
                    else GameManager.GetMonoSystem<IGPSMonoSystem>().TurnOn();
                    msg = new ConsoleResponse($"The GPS has been turned {((value == 0) ? "off" : "on")}.", ResponseType.Response);
                    return true;
                }
                else
                {
                    msg = new ConsoleResponse("GameManager is null.", ResponseType.Error);
                }
            }
            else
            {
                msg = new ConsoleResponse("Argument must be 0 or 1.", ResponseType.Error);
            }

            return false;
        }
    }
}
