using PlazmaGames.Core;
using PlazmaGames.UI;
using UnityEngine;

namespace HTJ21
{
    public class SettingsView : View
    {
        [SerializeField] private EventButton _back;

        private void Back()
        {
            GameManager.GetMonoSystem<IUIMonoSystem>().ShowLast();
        }

        public override void Init()
        {
            _back.onPointerDown.AddListener(Back);
        }
    }
}
