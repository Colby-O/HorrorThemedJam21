using PlazmaGames.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HTJ21
{
    public class ChoiceView : View
    {
        [SerializeField] private EventButton _save;
        [SerializeField] private EventButton _sacrifice;

        [SerializeField] private List<GameObject> _icons;
        [SerializeField] private List<TMP_Text> _labels;

        private void Save()
        {

        }

        private void Sacrifice()
        {

        }

        public override void Init()
        {
            _save.onPointerDown.AddListener(Save);
            _sacrifice.onPointerDown.AddListener(Sacrifice);

            _save.Icon = _icons[0];
            _sacrifice.Icon = _icons[1];
            foreach (GameObject icon in _icons) icon.SetActive(false);

            _save.Text = _labels[0];
            _save.Text.color = _save.colors.disabledColor;
            _sacrifice.Text = _labels[1];
            _sacrifice.Text.color = _sacrifice.colors.disabledColor;
        }

        public override void Show()
        {
            base.Show();

            HTJ21GameManager.IsPaused = true;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            HTJ21GameManager.UseCustomCursor();
        }
    }
}
