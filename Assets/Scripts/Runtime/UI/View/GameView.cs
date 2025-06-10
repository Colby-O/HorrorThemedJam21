using PlazmaGames.UI;
using TMPro;
using UnityEngine;

namespace HTJ21
{
    public class GameView : View
    {
        [SerializeField] private TMP_Text _hint;

        public void SetHint(string hint)
        {
            _hint.text = hint;
            _hint.transform.parent.gameObject.SetActive(true);
        }

        public void HideHint()
        {
            _hint.text = string.Empty;
            _hint.transform.parent.gameObject.SetActive(false);
        }

        public override void Init()
        {

        }

        public override void Show()
        {
            base.Show();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            HideHint();
        }

        private void LateUpdate()
        {
            HideHint();
        }
    }
}
