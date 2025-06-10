using PlazmaGames.UI;
using UnityEngine;

namespace HTJ21
{
    public class GameView : View
    {
        public override void Init()
        {

        }

        public override void Show()
        {
            base.Show();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
