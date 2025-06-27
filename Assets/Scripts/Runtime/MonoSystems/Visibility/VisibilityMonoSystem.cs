using PlazmaGames.Attribute;
using PlazmaGames.Core;
using PlazmaGames.Core.Debugging;
using PlazmaGames.Runtime.DataStructures;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class VisibilityMonoSystem : MonoBehaviour, IVisibilityMonoSystem
    {
        [SerializeField, ReadOnly] private VisiblilityManifest _manifest;

        public void Load(Act act)
        {
            if (!_manifest)
            {
                PlazmaDebug.LogWarning("Visibleility Manifest was not assigned.", "Visibility", 1, Color.yellow);
                return;
            }

            List<GameObject> _visibleObjects = new List<GameObject>();

            foreach (Act a in _manifest.Manifest.Keys)
            {
                foreach (GameObject obj in _manifest.Manifest[a])
                {
                    if (a == act && !_visibleObjects.Contains(obj)) _visibleObjects.Add(obj);
                    obj.SetActive(false);
                }
            }

            foreach (GameObject obj in _visibleObjects) obj.SetActive(true);
        }

        private void Start()
        {
            _manifest = FindAnyObjectByType<VisiblilityManifest>();
            Load(GameManager.GetMonoSystem<IDirectorMonoSystem>().GetStartAct());
        }
    }
}
