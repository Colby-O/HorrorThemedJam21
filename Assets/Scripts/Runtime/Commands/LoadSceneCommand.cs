using PlazmaGames.Console;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "LoadSceneCommand", menuName = "Console Commands/Scene/Load")]
    public class LoadSceneCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            if (args.Length == 0)
            {
                msg = new ConsoleResponse("Must pass a scene name.", ResponseType.Error);
                return false;
            }

            if (Application.CanStreamedLevelBeLoaded(args[0]))
            {
                msg = new ConsoleResponse($"Changing scene to '{args[0]}'.", ResponseType.Response);
                SceneManager.LoadScene(args[0]);
            }
            else
            {
                msg = new ConsoleResponse($"The scene '{args[0]}' is invalid.", ResponseType.Error);
                return false;
            }

            return true;
        }
    }
}
