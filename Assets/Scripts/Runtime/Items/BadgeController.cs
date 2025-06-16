using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class BadgeController : MonoBehaviour
    {
        [SerializeField] private Keypad _linkedLock;

        [SerializeField] private List<MeshRenderer> _hints;

        private void Start()
        {
            KeypadPattern pattern = _linkedLock.GetCorrectPattern();

            for (int i = 0; i < 9; i++)
            {
                if (pattern.Get(i)) _hints[i].material.SetColor("_BaseColor", Color.red);
                else _hints[i].material.SetColor("_BaseColor", Color.black);
            }
        }
    }
}
