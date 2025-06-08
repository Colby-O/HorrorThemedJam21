
using System;
using System.Collections.Generic;
using PlazmaGames.Attribute;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines;

namespace HTJ21
{
    [Serializable]
    struct GPSNode
    {
        public SplineContainer container;
        public int splineIndex;
        public int knotIndex;
        public Vector3 position;

        public GPSNode(SplineContainer container, int splineIndex, int knotIndex, Vector3 position)
        {
            this.container = container;
            this.splineIndex = splineIndex;
            this.knotIndex = knotIndex;
            this.position = position;
        }

        public override bool Equals(object obj)
        {
            if (obj is GPSNode other) 
            {
                return splineIndex == other.splineIndex && knotIndex == other.knotIndex;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return splineIndex * 397 ^ knotIndex;
        }
    }

    public class GPSMonoSystem : MonoBehaviour, IGPSMonoSystem
    {
        [SerializeField, ReadOnly] private bool _isOn;
        [SerializeField, ReadOnly] private Transform _target;
        [SerializeField, ReadOnly] private GPSNode _targetNode;
        [SerializeField, ReadOnly] private List<Roadway> _roadways;

        private GPSNode GetClosestNodeToPoint(List<Roadway> roadways, Vector3 target)
        {
            float minDst = float.MaxValue;
            GPSNode minNode = new GPSNode(null, -1, -1, Vector3.zero);

            foreach (Roadway road in roadways)
            {
                Spline spline = road.container.Splines[road.splineIndex];
                for (int i = 0; i < spline.Count; i++)
                {
                    BezierKnot knot = spline[i];
                    Vector3 position = road.container.transform.TransformPoint(knot.Position);
                    float dst = Vector3.Distance(target, position);

                    if (dst < minDst)
                    {
                        minNode = new GPSNode(road.container, road.splineIndex, i, position);
                        minDst = dst;
                    }
                }
            }

            return minNode;
        }

        private List<GPSNode> GetConnectedNodes(GPSNode node)
        {
            List<GPSNode> neighbors = new List<GPSNode>();

            if (node.splineIndex < 0 || node.splineIndex >= node.container.Splines.Count) return new List<GPSNode>();

            Spline spline = node.container.Splines[node.splineIndex];

            if (node.knotIndex > 0) neighbors.Add(new GPSNode(node.container, node.splineIndex, node.knotIndex - 1, node.container.transform.TransformPoint(spline[node.knotIndex - 1].Position)));
            if (node.knotIndex < spline.Count - 1) neighbors.Add(new GPSNode(node.container, node.splineIndex, node.knotIndex + 1, node.container.transform.TransformPoint(spline[node.knotIndex + 1].Position)));

            //TODO: Add  Itersection

            return neighbors;
        }

        private List<GPSNode> PathFindToTarget(GPSNode start)
        {
            List<GPSNode> path = new List<GPSNode>();

            HashSet<GPSNode> visted = new HashSet<GPSNode>();
            Queue<GPSNode> queue = new Queue<GPSNode>();
            Dictionary<GPSNode, GPSNode> cameFrom = new Dictionary<GPSNode, GPSNode>();

            queue.Enqueue(start);
            visted.Add(start);
            while (queue.Count > 0) 
            { 
                GPSNode current = queue.Dequeue();
                if (current.Equals(_target)) return ReconstructPath(cameFrom, current);

                foreach (GPSNode neighbor in GetConnectedNodes(current)) 
                { 
                    if (!visted.Contains(neighbor))
                    {
                        visted.Add(neighbor);
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return null;
        }

        private List<GPSNode> ReconstructPath(Dictionary<GPSNode, GPSNode> cameFrom, GPSNode current)
        {
            List<GPSNode> path = new List<GPSNode>() { current };
            while (cameFrom.ContainsKey(current)) 
            {
                Debug.DrawLine(current.position, cameFrom[current].position, Color.blue, Mathf.Infinity);
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        private void UpdateGPS()
        {
            GPSNode node = GetClosestNodeToPoint(_roadways, HTJ21GameManager.Player.transform.position);

            if (HTJ21GameManager.Player != null && node.container != null)
            {
                Debug.DrawLine(HTJ21GameManager.Player.transform.position, node.position, Color.red, 0.1f);
                Debug.Log($"Player: {HTJ21GameManager.Player.transform.position} Knot: {node.position} Distance: {Vector3.Distance(HTJ21GameManager.Player.transform.position, node.position)}");
            }
            else Debug.Log("Error!");
        }

        private void Awake()
        {
            _roadways = RoadwayHelper.GetRoadways();
            _target = GameObject.FindWithTag("GPSTarget").transform;
            if (_target != null) _targetNode = GetClosestNodeToPoint(_roadways, _target.position);
        }

        private void Start()
        {
            PathFindToTarget(GetClosestNodeToPoint(_roadways, HTJ21GameManager.Player.transform.position));
        }

        private void Update()
        {
            UpdateGPS();
        }
    }
}
