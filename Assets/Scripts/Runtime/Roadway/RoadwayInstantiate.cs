using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PlazmaGames.Attribute;
using Unity.Mathematics;
using UnityEngine.Splines;

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
			public GameObject prefab;
			public bool left, right;
			public float step;
			public float frequency;
			public bool randomOffset;
			public Vector3 offsetFrom;
			public Vector3 offsetTo;
		}

		[SerializeField] private List<InstanceInfo> _instances = new();
		[SerializeField] private List<Section> _sections = new();

		private List<List<Matrix4x4[]>> _instanceMatrices = new();
		private Matrix4x4[] _tempMatrices = new Matrix4x4[100];
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
			_instanceMatrices = new List<List<Matrix4x4[]>>();
			
			float roadWidth = RoadwayCreator.Instance.RoadWidth();
			foreach (Section s in _sections)
			{
				List<Matrix4x4[]> instanceMatrices = new();
				_instanceMatrices.Add(instanceMatrices);
				foreach (InstanceInfo inst in _instances)
				{
					bool leftOn = inst.left && s.left;
					bool rightOn = inst.right && s.right;
					float tStart = RoadwayHelper.GetKnotTInSpline(s.container, s.spline, s.knotStart);
					float tLength = RoadwayHelper.GetTBetweenKnots(s.container, s.spline, s.knotStart, s.knotEnd);
					int segmentCount = Mathf.FloorToInt(RoadwayHelper.GetDistanceBetweenKnots(s.container, s.spline, s.knotStart, s.knotEnd) / inst.step);
					Matrix4x4[] matrices = new Matrix4x4[(leftOn ? segmentCount : 0) + (rightOn ? segmentCount : 0)];
					instanceMatrices.Add(matrices);
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
							matrices[i] = Matrix4x4.TRS(leftPos, Quaternion.LookRotation(forward, up), inst.prefab.transform.localScale);
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
							int index = i;
							if (leftOn) index += segmentCount;
							matrices[index] = Matrix4x4.TRS(rightPos, Quaternion.LookRotation(forward, up), inst.prefab.transform.localScale);
						}
					}
				}
			}
		}

		private void Start()
		{
			Generate();
		}

		private void Update()
		{
			if (_instanceMatrices.Count == 0) return;

			foreach (var instanceMatrices in _instanceMatrices)
			{
				for (int instId = 0; instId < _instances.Count; instId++)
				{
					InstanceInfo inst = _instances[instId];
					Matrix4x4[] matrices = instanceMatrices[instId];
					foreach (MeshRenderer renderer in inst.prefab.GetComponentsInChildren<MeshRenderer>())
					{
						MeshFilter filter = renderer.GetComponent<MeshFilter>();
						if (!filter) continue;
						Mesh mesh = filter.sharedMesh;
						Material[] materials = renderer.sharedMaterials;

						Transform child = renderer.transform;
						Matrix4x4 localMatrix = inst.prefab.transform.worldToLocalMatrix * child.localToWorldMatrix;

						for (int i = 0; i < mesh.subMeshCount && i < materials.Length; i++)
						{
							Material material = materials[i];
							if (material.enableInstancing == false)
							{
								Material clone;
								if (_materialClones.TryGetValue(material, out clone))
								{
									material = clone;
								}
								else
								{
									clone = new Material(material);
									clone.enableInstancing = true;
									_materialClones[material] = clone;
									material = clone;
								}
							}

							EnsureTempMatricesSize(matrices.Length);
							for (int j = 0; j < matrices.Length; j++)
							{
								_tempMatrices[j] = matrices[j] * localMatrix;
							}

							Graphics.DrawMeshInstanced(mesh, i, material, _tempMatrices[0..matrices.Length]);
						}
					}
				}
			}
		}

		private void EnsureTempMatricesSize(int matricesLength)
		{
			if (_tempMatrices.Length < matricesLength)
			{
				_tempMatrices = new Matrix4x4[matricesLength];
			}
		}
	}
}

