using PlazmaGames.Runtime.DataStructures;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "RoadwaySO", menuName = "Roadway/RoadwaySO")]
    public class RoadwaySO : ScriptableObject
    {
        public SerializableDictionary<string, List<RoadwayIntersection>> intersections = new SerializableDictionary<string, List<RoadwayIntersection>>();
    }
}
