using System;
using System.Collections.Generic;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor.Splines;
#endif
using UnityEngine;
using UnityEngine.Splines;

namespace HTJ21
{
    [Serializable]
    public struct Roadway
    {
        public SplineContainer container;
        public int splineIndex;
        public List<float> segments;
    }

    public class RoadwayHelper : MonoBehaviour
    {

        public static void GetRoadwayWidthAt(SplineContainer roadway, int splineIndex, float t, float width, out Vector3 p1, out Vector3 p2)
        {
            roadway.Evaluate(splineIndex, t, out float3 position, out float3 forward, out float3 up);
            float3 right = Vector3.Cross(Vector3.Normalize(forward), up);
            p1 = position + width * right;
            p2 = position - width * right;
        }

        public static List<SplineContainer> GetRoadwayContainers()
        {
            List<SplineContainer> roadwayContainers = new List<SplineContainer>();

            GameObject[] paths = GameObject.FindGameObjectsWithTag("RoadwayPath");
            for (int i = 0; i < paths.Length; i++)
            {
                GameObject go = paths[i];
                if (go.TryGetComponent(out SplineContainer splineContainer))
                {
                    roadwayContainers.Add(splineContainer);
                }
            }

            return roadwayContainers;
        }

        public static List<Roadway> GetRoadways()
        {
            List<Roadway> roadways = new List<Roadway>();

            List <SplineContainer> roadwayContainers = GetRoadwayContainers();

            foreach (SplineContainer splineContainer in roadwayContainers)
            {
                    for (int i = 0; i < splineContainer.Splines.Count; i++)
                    {
                        Roadway roadway = new Roadway();
                        roadway.container = splineContainer;
                        roadway.splineIndex = i;
                        roadways.Add(roadway);
                    }
            }

            return roadways;
        }

#if UNITY_EDITOR

        public static List<SelectableKnot> GetSelectedRoadwayKnots()
        {
            List<SplineContainer> roadwayContainers = GetRoadwayContainers();
            List<SelectableKnot> selectedKnots = new List<SelectableKnot>();

            if (SplineSelection.Count > 0)
            {
                foreach (SplineContainer splineContainer in roadwayContainers)
                {
                    for (int i = 0; i < splineContainer.Splines.Count; i++)
                    {
                        List<SelectableKnot> selected = new List<SelectableKnot>();
                        SplineInfo info = new SplineInfo(splineContainer, i);
                        SplineSelection.GetElements(info, selected);

                        for (int j = selected.Count - 1; j >= 0; j--)
                        {
                            SelectableKnot knot  = selected[j];
                            if (knot.KnotIndex != 0 && knot.KnotIndex != knot.SplineInfo.Spline.Count - 1) selected.RemoveAt(j);
                        }

                        selectedKnots.AddRange(selected);
                    }
                }
            }

            return selectedKnots;
        }

#endif
    }
}
