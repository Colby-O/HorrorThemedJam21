
using System;
using System.Collections.Generic;
using PlazmaGames.Attribute;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using Utils;

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
        [Header("Settings")]
        [SerializeField, Min(1)] private int _gpsResolution;

        [SerializeField, ReadOnly] private bool _isOn = true;
        [SerializeField, ReadOnly] private Transform _target;
        [SerializeField, ReadOnly] private GPSNode _targetNode;
        [SerializeField, ReadOnly] private List<Roadway> _roadways;
        [SerializeField, ReadOnly] private List<GPSNode> _currentPath;


        private LineRenderer _renderer;

        public Transform GetTarget() => _target;

        public void TurnOn() => _isOn = true;

        public void TurnOff() => _isOn = false;

        public Vector3 GetClosestNodePositionToPoint(List<Roadway> roadways, Vector3 target)
        {
            return GetClosestNodeToPoint(roadways, target).position;
        }

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

            List<RoadwayIntersection> intersections = RoadwayCreator.Instance.GetIntersections();

            foreach (RoadwayIntersection intersection in intersections)
            {
                JunctionInfo thisJunction = new JunctionInfo(node.splineIndex, node.knotIndex, node.container);
                if (intersection.HasJunction(thisJunction))
                {
                    foreach (JunctionInfo junction in intersection.GetJunctions())
                    {
                        if (!junction.Equals(thisJunction))
                        {
                            neighbors.Add(new GPSNode(junction.splineContainer, junction.splineIndex, junction.knotIndex, junction.splineContainer.transform.TransformPoint(junction.splineContainer[junction.splineIndex][junction.knotIndex].Position)));
                        }
                    }
                }
            }

            return neighbors;
        }

        private float GetDistanceBetween(GPSNode a, GPSNode b)
        {
            if (a.splineIndex != b.splineIndex || a.container != b.container) 
            {
                return Vector3.Distance(a.position, b.position);
            }

            var spline = a.container.Splines[a.splineIndex];
            int start = Mathf.Min(a.knotIndex, b.knotIndex);
            int end = Mathf.Max(a.knotIndex, b.knotIndex);

            return RoadwayHelper.GetDistanceBetweenKnots(a.container, a.splineIndex, start, end); 
        }

        private List<GPSNode> PathFindToTarget(GPSNode start)
        {
            if (start.Equals(_targetNode)) return new List<GPSNode>() { start };

            List<GPSNode> path = new List<GPSNode>();

            PriorityQueue<GPSNode, float> queue = new PriorityQueue<GPSNode, float>();
            Dictionary<GPSNode, GPSNode> cameFrom = new Dictionary<GPSNode, GPSNode>();
            Dictionary<GPSNode, float> costSoFar = new Dictionary<GPSNode, float>();

            queue.Enqueue(start, 0);
            costSoFar[start] = 0;

            while (queue.Count > 0) 
            { 
                GPSNode current = queue.Dequeue();
                if (current.Equals(_targetNode)) return ReconstructPath(cameFrom, current);

                foreach (GPSNode neighbor in GetConnectedNodes(current)) 
                {
                    float cost = GetDistanceBetween(current, neighbor);
                    float newCost = costSoFar[current] + cost;

                    if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                    {
                        costSoFar[neighbor] = newCost;
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor, newCost);
                    }
                }
            }

            return new List<GPSNode>();
        }

        private List<GPSNode> ReconstructPath(Dictionary<GPSNode, GPSNode> cameFrom, GPSNode current)
        {
            List<GPSNode> path = new List<GPSNode>() { current };
            while (cameFrom.ContainsKey(current)) 
            {
                Debug.DrawLine(current.position, cameFrom[current].position, Color.green, 1f);
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        private void UpdateGPS()
        {
            if (!_isOn || !HTJ21GameManager.Car) return;

            _currentPath = PathFindToTarget(GetClosestNodeToPoint(_roadways, HTJ21GameManager.Car.transform.position));
            DrawPath(_currentPath);
        }

        private void DrawPath(List<GPSNode> nodes)
        {
            if (nodes.Count == 0) return;
            
            float offsetY = 10f;

            Queue<GPSNode> queue = new Queue<GPSNode>(nodes);

            List<Vector3> points = new List<Vector3>();

            if (HTJ21GameManager.Car != null)
            {
                Vector3 playerPos = HTJ21GameManager.Car.transform.position;
                Vector3 start = queue.Peek().position;

                playerPos.y += offsetY;
                start.y += offsetY;

                points.Add(playerPos);
                points.Add(start);
            }
            
            if (queue.Count <= 1) return;

            while (queue.Count > 1) 
            {
                GPSNode startNode = queue.Dequeue();
                GPSNode endNode = queue.Peek();

                if (startNode.splineIndex == endNode.splineIndex && startNode.container == endNode.container) 
                {
                    Spline spline = startNode.container[startNode.splineIndex];

                    int start = startNode.knotIndex;
                    int end = endNode.knotIndex;

                    float tStart = RoadwayHelper.GetKnotTInSpline(startNode.container, startNode.splineIndex, startNode.knotIndex);
                    float tEnd = RoadwayHelper.GetKnotTInSpline(endNode.container, endNode.splineIndex, endNode.knotIndex);

                    for (int j = 0; j < _gpsResolution; j++)
                    {
                        float t = Mathf.Lerp(tStart, tEnd, j / (float)_gpsResolution);
                        Vector3 pos = startNode.container.EvaluatePosition(startNode.splineIndex, t);
                        pos.y += offsetY;
                        points.Add(pos);
                    }
                }
                else
                {
                    Vector3 posStart = startNode.container.transform.TransformPoint(startNode.container[startNode.splineIndex][startNode.knotIndex].Position);
                    Vector3 posEnd = endNode.container.transform.TransformPoint(endNode.container[endNode.splineIndex][endNode.knotIndex].Position);

                    posStart.y += offsetY;
                    posEnd.y += offsetY;

                    points.Add(posStart);
                    points.Add(posEnd);
                }
            }

            _renderer.positionCount = points.Count;
            _renderer.SetPositions(points.ToArray());
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            _roadways = RoadwayHelper.GetRoadways();
            _target = GameObject.FindWithTag("GPSTarget")?.transform;

            _renderer = GameObject.FindWithTag("GPSPath").GetComponent<LineRenderer>();
            if (_renderer == null)
            {
                GameObject path = new GameObject("GPSPath");
                _renderer = path.AddComponent<LineRenderer>();
            }

            SyncTarget();
            
            //TODO: Remove Me
            TurnOn();
        }

        public void MoveTarget(Vector3 position)
        {
            _target.position = position;
            SyncTarget();
        }

        private void SyncTarget()
        {
            if (_target != null) _targetNode = GetClosestNodeToPoint(_roadways, _target.position);
        }

        private void Start()
        {

        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }

        private void Update()
        {
            UpdateGPS();
        }
    }
}
