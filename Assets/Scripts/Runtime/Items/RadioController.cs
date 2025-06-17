using PlazmaGames.Attribute;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HTJ21
{
    [System.Serializable]
    public struct RadioStation
    {
        public string stationName;
        public string songName;
        public AudioClip audioClip;

        public RadioStation(string stationName, string songName, AudioClip clip)
        {
            this.stationName = stationName;
            this.songName = songName;
            this.audioClip = clip;
        }
    }

    public class RadioController : MonoBehaviour
    {
        [SerializeField] private List<RadioStation> _stationInput;
        [SerializeField] private Camera _camera;
        [SerializeField] private float _renderTime = 0.1f;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private KeypadButton _button;
        [SerializeField] private ScrolllingText _scroll;
        [SerializeField, ReadOnly] private TMP_Text _stationInfo;

        [SerializeField, ReadOnly] private List<RadioStation> _stations;
        [SerializeField, ReadOnly] private RadioStation _currentStation;
        [SerializeField, ReadOnly] private int _currentID;
        [SerializeField, ReadOnly] private float _currentTime; 

        private void PlayStation(RadioStation station)
        {
            _audioSource.Stop();
            _audioSource.clip = _currentStation.audioClip;
            _audioSource.Play();

        }

        private void UpdateInfo(RadioStation station)
        {
            _stationInfo = _scroll.GetFirst();
            _stationInfo.text = $" {station.stationName} -- {station.songName} ";
            _scroll.OnTextUpdate();
        }

        private void NextStation()
        {
            int next =(_currentID + 1) % _stations.Count;
            _currentID = next;
            _currentStation = _stations[next];
            PlayStation(_currentStation);
            UpdateInfo(_currentStation);
        }

        private void Awake()
        {
            _currentTime = 0;
            _camera.enabled = false;
            _audioSource.loop = true;
            _stations = new List<RadioStation>() { new RadioStation(string.Empty, string.Empty, null) };
            _stations.AddRange(_stationInput);
            
            _currentID = -1;
            NextStation();
            _button.OnClick += NextStation;
        }

        public void Update()
        {
            _currentTime += Time.deltaTime;

            if (_currentTime >= _renderTime)
            {
                _camera.Render();
                _currentTime = 0;
            }
        }
    }
}
