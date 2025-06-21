using PlazmaGames.Core;
using PlazmaGames.Runtime.DataStructures;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class HouseController : MonoBehaviour
    {
        [Header("Sections")]
        [SerializeField] private SerializableDictionary<GameObject, List<Act>> _sections;

        public void OnActChange()
        {
            Act act = GameManager.GetMonoSystem<IDirectorMonoSystem>().GetCurrentAct();

            foreach (GameObject section in _sections.Keys)
            {
                if (!_sections[section].Contains(act)) section.SetActive(false);
                else section.SetActive(true);
            }
        }

        public void EnableHouse()
        {
            foreach (GameObject section in _sections.Keys)
            {
                section.SetActive(true);
            }
        }

        private void Start()
        {
            OnActChange();
        }
    }
}
