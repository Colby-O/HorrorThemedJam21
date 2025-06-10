using System.Collections.Generic;
using UnityEngine;
using PlazmaGames.Attribute;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine.Splines;
using Random = Unity.Mathematics.Random;

#if UNITY_EDITOR
using UnityEditor.Splines;
#endif

namespace HTJ21
{
	[ExecuteInEditMode]
	public class RoadwayInstantiate : MonoBehaviour
	{
		[System.Serializable]
		class Section
		{
			public SplineContainer container;
			public int spline;
			public int knotStart, knotEnd;
			public bool left, right;

			public Section(SplineContainer container, int spline, int knotStart, int knotEnd)
			{
				this.container = container;
				this.spline = spline;
				this.knotStart = knotStart;
				this.knotEnd = knotEnd;
				this.left = true;
				this.right = true;
			}
		}

		[System.Serializable]
		public class InstanceInfo
        {
            public int seed;
			public GameObject prefab;
			public bool left, right;
			public float step;
			public float frequency;
			public bool randomOffset;
			public Vector3 offsetFrom;
			public Vector3 offsetTo;
            public float viewDistance;
        }
        
		[SerializeField] private List<InstanceInfo> _instances = new();
		[SerializeField] private List<Section> _sections = new();

        class DrawnPart
        {
            public Matrix4x4 localMatrix;
            public Mesh mesh;
            public Material[] materials;
        }

        class DrawnInstance
        {
            public Matrix4x4[] matrices;
            public List<DrawnPart> parts = new();
            public float viewDistanceSq;
        }

        class DrawnSection
        {
            public MinMaxAABB bounds = new();
            public List<DrawnInstance> instances = new();
            public float viewDistance = 0;
        }

        private List<DrawnSection> _drawnSections = new();
		private Dictionary<Material, Material> _materialClones = new();

#if UNITY_EDITOR
		[InspectorButton("CreateSection")] public bool buttonCreateSection = false;
		void CreateSection()
		{
			List<SelectableKnot> knots = RoadwayHelper.GetSelectedRoadwayKnots(false);
			if (knots.Count != 2) return;
			_sections.Add(new Section(knots[0].SplineInfo.Container as SplineContainer, knots[0].SplineInfo.Index, knots[0].KnotIndex, knots[1].KnotIndex));
		}
		
		[InspectorButton("Generate")] public bool buttonGenerate = false;
#endif
		void Generate()
        {
            int pSeed = Mathf.FloorToInt(UnityEngine.Random.value * int.MaxValue);
            
			_drawnSections = new ();
			
			float roadWidth = RoadwayCreator.Instance.RoadWidth();
			foreach (Section s in _sections)
			{
                DrawnSection ds = new();
				_drawnSections.Add(ds);
				foreach (InstanceInfo inst in _instances)
                {
                    UnityEngine.Random.InitState(inst.seed);
                    if (ds.viewDistance < inst.viewDistance) ds.viewDistance = inst.viewDistance;
					bool leftOn = inst.left && s.left;
					bool rightOn = inst.right && s.right;
					float tStart = RoadwayHelper.GetKnotTInSpline(s.container, s.spline, s.knotStart);
					float tLength = RoadwayHelper.GetTBetweenKnots(s.container, s.spline, s.knotStart, s.knotEnd);
					int segmentCount = Mathf.FloorToInt(RoadwayHelper.GetDistanceBetweenKnots(s.container, s.spline, s.knotStart, s.knotEnd) / inst.step);
                    DrawnInstance di = new();
                    ds.instances.Add(di);
                    di.viewDistanceSq = inst.viewDistance * inst.viewDistance;
                    LoadDrawnParts(inst, di);
					di.matrices = new Matrix4x4[(leftOn ? segmentCount : 0) + (rightOn ? segmentCount : 0)];
					for (int i = 0; i < segmentCount; i++)
					{
						float t = tStart + i / (float)segmentCount * tLength;
						Vector3 leftEdge, rightEdge; 
						RoadwayHelper.GetRoadwayWidthAt(s.container, s.spline, t, roadWidth, out leftEdge, out rightEdge);
						float3 tangentF3, upF3;
						s.container[s.spline].Evaluate(t, out var _, out tangentF3, out upF3);
						Vector3 forward = Vector3.Normalize(tangentF3);
						Vector3 up = Vector3.Normalize(upF3);
						Vector3 right = Vector3.Cross(forward, up);
						Vector3 offset;
						if (inst.randomOffset)
						{
							offset =
								right * UnityEngine.Random.Range(inst.offsetFrom.x, inst.offsetTo.x) +
								up * UnityEngine.Random.Range(inst.offsetFrom.y, inst.offsetTo.y) +
								forward * UnityEngine.Random.Range(inst.offsetFrom.z, inst.offsetTo.z);
						}
						else
						{
							offset =
								right * inst.offsetFrom.x +
								up * inst.offsetFrom.y +
								forward * inst.offsetFrom.z;
						}

						if (leftOn)
						{
							Vector3 leftPos = leftEdge + offset;
                            ds.bounds.Encapsulate(leftPos);
							di.matrices[i] = Matrix4x4.TRS(leftPos, Quaternion.LookRotation(forward, up), inst.prefab.transform.localScale);
						}
						
						if (inst.randomOffset)
						{
							offset =
								-right * UnityEngine.Random.Range(inst.offsetFrom.x, inst.offsetTo.x) +
								up * UnityEngine.Random.Range(inst.offsetFrom.y, inst.offsetTo.y) +
								forward * UnityEngine.Random.Range(inst.offsetFrom.z, inst.offsetTo.z);
						}
						else
						{
							offset =
								-right * inst.offsetFrom.x +
								up * inst.offsetFrom.y +
								forward * inst.offsetFrom.z;
						}
						
						if (rightOn)
						{
							Vector3 rightPos = rightEdge + offset;
                            ds.bounds.Encapsulate(rightPos);
							int index = i;
							if (leftOn) index += segmentCount;
							di.matrices[index] = Matrix4x4.TRS(rightPos, Quaternion.LookRotation(forward, up), inst.prefab.transform.localScale);
						}
					}
				}
			}
            
            UnityEngine.Random.InitState(pSeed);
		}

        private void LoadDrawnParts(InstanceInfo inst, DrawnInstance di)
        {
            foreach (MeshRenderer mr in inst.prefab.GetComponentsInChildren<MeshRenderer>())
            {
                DrawnPart p = new();
                di.parts.Add(p);
                MeshFilter mf = mr.GetComponent<MeshFilter>();
                p.mesh = mf.sharedMesh;
                p.localMatrix = inst.prefab.transform.worldToLocalMatrix * mr.localToWorldMatrix;
                p.materials = mr.sharedMaterials;
                for (int i = 0; i < p.materials.Length; i++)
                {
                    if (p.materials[i].enableInstancing) continue;
                    if (_materialClones.TryGetValue(p.materials[i], out Material clone))
                    {
                        p.materials[i] = clone;
                    }
                    else
                    {
                        clone = new Material(p.materials[i]);
                        clone.enableInstancing = true;
                        _materialClones.Add(p.materials[i], clone);
                        p.materials[i] = clone;
                    }
                }
            }
        }

        private void Start()
		{
			Generate();
		}

        private List<Matrix4x4> _drawn = new(256);
		private void Update()
		{
            foreach (DrawnSection s in _drawnSections)
            {
                float d = s.viewDistance;
                if (HTJ21GameManager.Player && !s.bounds.Overlaps(MinMaxAABB.CreateFromCenterAndHalfExtents(HTJ21GameManager.Player.transform.position, new float3(d, d, d))))
                    continue;
                foreach (DrawnInstance inst in s.instances)
                {
                    foreach (DrawnPart part in inst.parts)
                    {
                        _drawn.Clear();
                        foreach (Matrix4x4 matrix in inst.matrices)
                        {
                            Vector3 pos = matrix.GetColumn(3);
                            if (HTJ21GameManager.Player && (HTJ21GameManager.Player.transform.position - pos).sqrMagnitude >= inst.viewDistanceSq) continue;
                            _drawn.Add(matrix * part.localMatrix);
                        }

                        for (int i = 0; i < part.materials.Length; i++)
                        {
                            Graphics.DrawMeshInstanced(part.mesh, i, part.materials[i], _drawn);
                        }
                    }
                }
            }
		}
	}
}

