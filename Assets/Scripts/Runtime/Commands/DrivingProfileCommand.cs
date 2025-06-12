using PlazmaGames.Console;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "DrivingProfileCommand", menuName = "Console Commands/Car/DrivingProfile")]
    public class DrivingProfileCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length != 1)
            {
                msg = new("You need to pass a profile name!", ResponseType.Error);
                return false;
            }

            if (!HTJ21GameManager.Instance || !HTJ21GameManager.Car)
            {
                msg = new("Car is null!", ResponseType.Error);
                return false;
            }

            if (!HTJ21GameManager.Car.SetDrivingProfile(args[0]))
            {
                msg = new("Invalid profile name!", ResponseType.Error);
                return false;
            }

            msg = new($"Driving profile has been set to '{args[0]}'", ResponseType.Response);
            return true;
        }
    }
}
