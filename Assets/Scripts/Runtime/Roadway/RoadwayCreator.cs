using PlazmaGames.Attribute;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

#if UNITY_EDITOR
using System.ComponentModel;
using UnityEditor.Callbacks;
using UnityEditor.Splines;
using UnityEditor.Timeline;
#endif

namespace HTJ21
{
    [ExecuteInEditMode]
    public class RoadwayCreator : MonoBehaviour
    {
        [Header("Generation Parameters")]
        [SerializeField, InspectorButton("GenerateRoadway")] private bool _regenerate = false;
        [SerializeField, InspectorButton("ClearIntersections")] private bool _removeAllIntersections = false;
        [SerializeField, InspectorButton("ClearRoadways")] private bool _destroy = false;
        [SerializeField] private bool _debugMode = false;
        [SerializeField] private float _resolution = 1;
        [SerializeField, PlazmaGames.Attribute.ReadOnly] private List<Roadway> _roadways;
        [SerializeField] private List<RoadwayIntersection> _intersections;

        [Header("Road Parameters")]
        [SerializeField] private float _roadWidth;
        [SerializeField] private float _curveWidth;
        [SerializeField] private float _curveHeight;

        [Header("Mesh Parameters")]
        [SerializeField] Material _roadMat;
        [SerializeField] Material _intersectionMat;
        [SerializeField] Material _curbMat;

        public static RoadwayCreator Instance { get; private set; }

        private GameObject _roadwayHolder;

        public List<Roadway> GetRoadways() => _roadways;
        public List<RoadwayIntersection> GetIntersections() => _intersections;

        public Transform GetRoadwayHolder() 
        {
            if (_roadwayHolder == null) _roadwayHolder = GameObject.Find("Roadway");
            if (_roadwayHolder == null) _roadwayHolder = new GameObject("Roadway");
            return _roadwayHolder.transform; 
        }

#if UNITY_EDITOR
        public RoadwayIntersection HasIntersectionWithAtLeastOneJunction(List<SelectableKnot> knots)
        {
            foreach (RoadwayIntersection intersection in _intersections)
            {
                if (intersection.HasAtLeastOneJunction(knots)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithAtLeastOneJunction(List<JunctionInfo> junctions)
        {
            foreach (RoadwayIntersection intersection in _intersections)
            {
                if (intersection.HasAtLeastOneJunction(junctions)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithJunctions(List<JunctionInfo> junctions, bool checkIfSame = true)
        {
            foreach (RoadwayIntersection intersection in _intersections)
            {
                if (intersection.HasJunctions(junctions, checkIfSame)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithJunctions(List<SelectableKnot> knots, bool checkIfSame = true)
        {
            foreach (RoadwayIntersection intersection in _intersections)
            {
                if (intersection.HasJunctions(knots, checkIfSame)) return intersection;
            }
            return null;
        }


        public void AddIntersection(RoadwayIntersection intersection)
        {
            if (_intersections == null) _intersections = new List<RoadwayIntersection>();
            RemoveIntersection(intersection);
           _intersections.Add(intersection);
        }

        public void RemoveIntersection(RoadwayIntersection intersection)
        {
            if (_intersections == null) return;
            if (_intersections.Contains(intersection)) _intersections.Remove(intersection);
        }

        public void ClearRoadways()
        {
            RoadwayMeshGenerator.Clear();
            if (_roadways == null) _roadways = new List<Roadway>();
            else _roadways.Clear();
            if (_roadwayHolder != null)
            {
                while (_roadwayHolder.transform.childCount > 0) DestroyImmediate(_roadwayHolder.transform.GetChild(0).gameObject);
            }
        }

        public void GenerateRoadway()
        {
            ClearRoadways();

            _roadways = RoadwayHelper.GetRoadways();

            for (int i = 0; i < _roadways.Count; i++)
            {
                Roadway roadway = _roadways[i];
                roadway.segments = new List<float>();
                float length = roadway.container.Splines[roadway.splineIndex].GetLength();
                int numberOfSegments = Mathf.CeilToInt(length / _resolution);
                for (float j = 0.0f; j <= numberOfSegments; j++) roadway.segments.Add(j / numberOfSegments);
                RoadwayMeshGenerator.GenerateRoadMesh(roadway, _roadWidth, _curveWidth, _curveHeight);
            }

            foreach (RoadwayIntersection intersection in _intersections) RoadwayMeshGenerator.GenerateIntersectionMesh(intersection, _roadWidth, _curveWidth, _curveHeight);
        }

        public void ClearIntersections()
        {
            _intersections.Clear();
            GenerateRoadway();
        }

        private void OnSplineChanged(Spline _, int __, SplineModification ___)
        {
            GenerateRoadway();
            //if (TerrainRoadwayConformer.Instance != null) TerrainRoadwayConformer.Instance.ConformTerrainToRoadway();
        }
#endif

        private void Awake()
        {
#if UNITY_EDITOR
            RoadwayMeshGenerator.Parent = GetRoadwayHolder();
            RoadwayMeshGenerator.RoadMat = _roadMat;
            RoadwayMeshGenerator.IntersectionMat = _intersectionMat;
            RoadwayMeshGenerator.CurbMat = _curbMat;
            Spline.Changed += OnSplineChanged;
#endif
            Instance = this;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            RoadwayMeshGenerator.Parent = GetRoadwayHolder();
            RoadwayMeshGenerator.RoadMat = _roadMat;
            RoadwayMeshGenerator.IntersectionMat = _intersectionMat;
            RoadwayMeshGenerator.CurbMat = _curbMat;
            Spline.Changed += OnSplineChanged;
#endif
            Instance = this;
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            Spline.Changed -= OnSplineChanged;
#endif
            Instance = null;
        }
        private void OnDestroy()
        {
#if UNITY_EDITOR
            Spline.Changed -= OnSplineChanged;
#endif
            Instance = null;
        }

#if UNITY_EDITOR
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            if (Instance != null) Instance.GenerateRoadway();
            //if (TerrainRoadwayConformer.Instance != null && TerrainRoadwayConformer.Instance.enabled) TerrainRoadwayConformer.Instance.ConformTerrainToRoadway();
        }
#endif
        public float RoadWidth() => _roadWidth;
    }
}

