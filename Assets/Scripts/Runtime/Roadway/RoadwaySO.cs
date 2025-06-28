using PlazmaGames.Runtime.DataStructures;
using System.Collections.Generic;
using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "RoadwaySO", menuName = "Roadway/RoadwaySO")]
    public class RoadwaySO : ScriptableObject
    {
        public List<RoadwayIntersection> intersections = new List<RoadwayIntersection>();
    }
}
