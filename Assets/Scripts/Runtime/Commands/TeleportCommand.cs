using PlazmaGames.Console;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "TeleportCommand", menuName = "Console Commands/Player/Teleport")]
    public class TeleportCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length != 1)
            {
                msg = new("You need to pass a location id to teleport to!", ResponseType.Error);
                return false;
            }

            GameObject[] locs = GameObject.FindGameObjectsWithTag("TeleportTarget");

            GameObject found = null;
            foreach (GameObject g in locs)
            {
                if (g.name == args[0])
                {
                    found = g;
                    break;
                }
            }

            if (found == null)
            {
                msg = new($"Invalid teleport location '{args[0]}'!", ResponseType.Error);
                return false;
            }

            msg = new($"Teleporting to '{args[0]}'!", ResponseType.Response);

            if (HTJ21GameManager.CurrentControllable)
            {
                HTJ21GameManager.CurrentControllable.transform.position = found.transform.position;
                HTJ21GameManager.CurrentControllable.transform.rotation = found.transform.rotation;
            }
            else
            {
                msg = new($"There is no controllable character in the scene!", ResponseType.Error);
                return false;
            }

            return true;
        }
    }
}
