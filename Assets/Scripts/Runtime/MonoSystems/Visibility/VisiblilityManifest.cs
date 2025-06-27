using PlazmaGames.Runtime.DataStructures;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    public class VisiblilityManifest : MonoBehaviour
    {
        public SerializableDictionary<Act, List<GameObject>> Manifest = new SerializableDictionary<Act, List<GameObject>>();
    }
}
