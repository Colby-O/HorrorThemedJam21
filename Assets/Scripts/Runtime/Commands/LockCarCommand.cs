using PlazmaGames.Console;
using PlazmaGames.Core;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "LockCarCommand", menuName = "Console Commands/Car/Lock")]
    public class LockCarCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length == 0)
            {
                msg = new ConsoleResponse("Must give a value between 0 and 1.", ResponseType.Error);
                return false;
            }
            if (int.TryParse(args[0], out int value))
            {
                if (HTJ21GameManager.Instance)
                {
                    if (value == 0) HTJ21GameManager.Car.Unlock();
                    else HTJ21GameManager.Car.Lock();
                    msg = new ConsoleResponse($"The car has been {((value == 0) ? "unlocked" : "locked")}.", ResponseType.Response);
                    return true;
                }
                else
                {
                    msg = new ConsoleResponse("GameManager is null.", ResponseType.Error);
                }
            }
            else
            {
                msg = new ConsoleResponse("Argument must be a 0 or 1.", ResponseType.Error);
            }

            return false;
        }
    }
}
