using PlazmaGames.Attribute;
using System;
using TMPro;
using UnityEngine;

namespace HTJ21
{
    public class AlarmClock : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private TMP_Text _timeDisplay;
        [SerializeField] private TMP_Text _nightDayDisplay;

        [SerializeField, ReadOnly] private DateTime _date;
        [SerializeField, ReadOnly] private int _lastMinute = -1;

        private void Awake()
        {
            _camera.enabled = false;
        }

        private void Update()
        {
            _date = DateTime.Now;
            int hour = _date.Hour;
            int minute = _date.Minute;
            string amOrPm = (hour > 12 && hour < 24) ? "PM" : "AM";

            _timeDisplay.text = $"{hour}:{minute.ToString("D2")}";
            _nightDayDisplay.text = amOrPm;

            if (minute != _lastMinute) _camera.Render();
            _lastMinute = minute;
        }
    }
}
