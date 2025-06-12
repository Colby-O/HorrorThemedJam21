using PlazmaGames.Console;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "ToggleSirenCommand", menuName = "Console Commands/Car/ToggleSiren")]
    public class ToggleSirenCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length == 0) msg = new ConsoleResponse("Must give 0 and 1.", ResponseType.Error);
            if (int.TryParse(args[0], out int value))
            {
                if (HTJ21GameManager.Instance)
                {
                    if (value == 0) HTJ21GameManager.Car.DisableSiren();
                    else HTJ21GameManager.Car.EnableSiren();
                    msg = new ConsoleResponse(ResponseType.None);
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

            return true;
        }
    }
}
