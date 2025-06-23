using PlazmaGames.Console;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "TeleportCommand", menuName = "Console Commands/Player/GiveLight")]
    public class GiveLightCommand : ConsoleCommand
    {
        public override bool Process(string[] args, out ConsoleResponse msg)
        {
            msg = new($"You now have a light!", ResponseType.Response);
            
            HTJ21GameManager.Player.GetComponent<PickupManager>().Pickup(PickupableItem.FlashLight);

            return true;
        }
    }
}
