using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.Runtime.DataStructures;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HTJ21
{
    public class HouseController : MonoBehaviour
    {
        [Header("Sections")]
        [SerializeField] private SerializableDictionary<GameObject, List<Act>> _sections;

        [SerializeField, ReadOnly] List<Light> _lights;
        [SerializeField, ReadOnly] List<Door> _doors;

        [SerializeField, ReadOnly] List<Light> _lightsToTurnBackOn;
        [SerializeField, ReadOnly] List<Door> _doorsToUnlock;
        public void TurnOffAllLights()
        {
            _lightsToTurnBackOn = new List<Light>();
            foreach (Light light in _lights)
            {
                if (light && light.gameObject.activeSelf)
                {
                    _lightsToTurnBackOn.Add(light);
                    light.gameObject.SetActive(false);
                }
            }
        }

        public void TurnOnLights()
        {
            foreach (Light light in _lightsToTurnBackOn)
            {
                if (light) light.gameObject.SetActive(true);
            }
        }

        public void LockAllDoors()
        {
            _doorsToUnlock = new List<Door>();

            foreach (Door door in _doors)
            {
                if (!door.IsLocked())
                {
                    door.Close();
                    door.Lock();
                    _doorsToUnlock.Add(door);
                }
            }
        }

        public void UnlockDoors()
        {
            foreach (Door door in _doorsToUnlock)
            {
                door.Unlock();
            }
        }

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

        private List<Light> GetAllLights()
        {
            return gameObject.GetComponentsInChildren<Light>(true).ToList();
        }

        private List<Door> GetAllDoors()
        {
            return gameObject.GetComponentsInChildren<Door>(true).ToList();
        }

        private void Start()
        {
            OnActChange();
            _lights = GetAllLights();
            _doors = GetAllDoors();
        }
    }
}
