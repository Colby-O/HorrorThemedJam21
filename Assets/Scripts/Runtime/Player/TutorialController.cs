using PlazmaGames.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private float _showTime = 3f;
        [SerializeField] private List<GameObject> _tutorials;

        [SerializeField, ReadOnly] private int _current = -1;
        [SerializeField, ReadOnly] private float _timer = 0;

        public void ShowTutorial(int id)
        {
            if (_tutorials == null || id >= _tutorials.Count) return;

            DisableAllTutorial();

            _tutorials[id].SetActive(true);
            _current = id;
            _timer = 0;

            gameObject.SetActive(true);
        }

        public void HideTutorial(int id)
        {
            if (_tutorials == null || id >= _tutorials.Count) return;

            _tutorials[id].SetActive(false);
            _current = -1;

            gameObject.SetActive(false);
        }

        public void DisableAllTutorial()
        {
            if (_tutorials == null) return;

            foreach(GameObject tutorial in _tutorials)
            {
                tutorial.SetActive(false);
            }

            _current = -1;

            gameObject.SetActive(false);
        }

        private void Awake()
        {
            DisableAllTutorial();
        }

        private void Update()
        {
            if (_current < 0 || HTJ21GameManager.IsPaused) return;

            _timer += Time.deltaTime;

            if (_timer > _showTime)
            {
                DisableAllTutorial();
            }
        }
    }
}
