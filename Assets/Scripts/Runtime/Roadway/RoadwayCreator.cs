using PlazmaGames.Attribute;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
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
        [SerializeField] private SplineContainer _splineContainer;
        [SerializeField] private bool _debugMode = false;
        [SerializeField] private float _resolution = 1;
        [SerializeField, ReadOnly] private List<Roadway> _roadways;
        [SerializeField] private RoadwaySO _data;

        [Header("Road Parameters")]
        [SerializeField] private float _roadWidth;
        [SerializeField] private float _curveWidth;
        [SerializeField] private float _curveHeight;

        [Header("Mesh Parameters")]
        [SerializeField] Material _roadMat;
        [SerializeField] Material _intersectionMat;
        [SerializeField] Material _curbMat;

        public static RoadwayCreator Instance { get; private set; }


        public SplineContainer GetContainer() => _splineContainer; 

        private GameObject _roadwayHolder;

        public List<Roadway> GetRoadways() => _roadways;

        public List<RoadwayIntersection> GetIntersections()
        {
            if (!_data.intersections.ContainsKey(gameObject.scene.name)) _data.intersections.Add(gameObject.scene.name, new List<RoadwayIntersection>());

            return _data.intersections[gameObject.scene.name];
        }

        public Transform GetRoadwayHolder() 
        {
            if (_roadwayHolder == null) _roadwayHolder = GameObject.Find("Roadway");
            if (_roadwayHolder == null) _roadwayHolder = new GameObject("Roadway");
            return _roadwayHolder.transform; 
        }

#if UNITY_EDITOR
        public RoadwayIntersection HasIntersectionWithAtLeastOneJunction(List<SelectableKnot> knots)
        {
            foreach (RoadwayIntersection intersection in GetIntersections())
            {
                if (intersection.HasAtLeastOneJunction(knots)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithAtLeastOneJunction(List<JunctionInfo> junctions)
        {
            foreach (RoadwayIntersection intersection in GetIntersections())
            {
                if (intersection.HasAtLeastOneJunction(junctions)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithJunctions(List<JunctionInfo> junctions, bool checkIfSame = true)
        {
            foreach (RoadwayIntersection intersection in GetIntersections())
            {
                if (intersection.HasJunctions(junctions, checkIfSame)) return intersection;
            }
            return null;
        }

        public RoadwayIntersection HasIntersectionWithJunctions(List<SelectableKnot> knots, bool checkIfSame = true)
        {
            foreach (RoadwayIntersection intersection in GetIntersections())
            {
                if (intersection.HasJunctions(knots, checkIfSame)) return intersection;
            }
            return null;
        }


        public void AddIntersection(RoadwayIntersection intersection)
        {
            RemoveIntersection(intersection);
            GetIntersections().Add(intersection);
        }

        public void RemoveIntersection(RoadwayIntersection intersection)
        {
            if (GetIntersections().Contains(intersection)) GetIntersections().Remove(intersection);
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

            _roadways = RoadwayHelper.GetRoadways(_splineContainer);

            for (int i = 0; i < _roadways.Count; i++)
            {
                Roadway roadway = _roadways[i];
                roadway.segments = new List<float>();
                float length = _splineContainer.Splines[roadway.splineIndex].GetLength();
                int numberOfSegments = Mathf.CeilToInt(length / _resolution);
                for (float j = 0.0f; j <= numberOfSegments; j++) roadway.segments.Add(j / numberOfSegments);
                RoadwayMeshGenerator.GenerateRoadMesh(roadway, _roadWidth, _curveWidth, _curveHeight);
            }

            foreach (RoadwayIntersection intersection in GetIntersections()) RoadwayMeshGenerator.GenerateIntersectionMesh(intersection, _roadWidth, _curveWidth, _curveHeight);
        }

        public void ClearIntersections()
        {
            GetIntersections().Clear();
            GenerateRoadway();
        }

        private void OnSplineChanged(Spline _, int __, SplineModification ___)
        {
            GenerateRoadway();
            //if (TerrainRoadwayConformer.Instance != null) TerrainRoadwayConformer.Instance.ConformTerrainToRoadway();
        }
#endif

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

#if UNITY_EDITOR
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            //if (Instance != null) Instance.GenerateRoadway();
            //if (TerrainRoadwayConformer.Instance != null && TerrainRoadwayConformer.Instance.enabled) TerrainRoadwayConformer.Instance.ConformTerrainToRoadway();
        }
#endif
        public float RoadWidth() => _roadWidth;
    }
}

