#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Splines;
using UnityEditor.Splines;
using PlazmaGames.Attribute;
using UnityEditor;
using PlazmaGames.Core;
using UnityEditor.ShaderGraph.Internal;
using System.Linq;

namespace HTJ21
{
    struct Edge
    {
        public Vector3 left;
        public Vector3 right;
        public Vector3 center;

        public Edge(Vector3 left, Vector3 right)
        {
            this.left = left;
            this.right = right;
            this.center = (right + left) / 2.0f;
        }
    }

    public class RoadwayMeshGenerator
    {
        public static List<GameObject> meshes { get; private set; }

        public static Transform parent { get; set; }
        public static Material mat { get; set; }

        public static void Clear()
        {
            if (meshes == null) return;
            if (meshes.Count > 0) foreach (GameObject go in meshes) if (go != null) GameObject.DestroyImmediate(go);
            meshes.Clear();
        }

        private static Vector3 GetIntersectionInnerEdges(RoadwayIntersection intersection, float roadWidth, ref List<Edge> edges, ref List<Vector3> points)
        {
            Vector3 center = Vector3.zero;

            foreach (JunctionInfo junction in intersection.GetJunctions())
            {
                float t = junction.knotIndex == 0 ? 0f : 1f;

                RoadwayHelper.GetRoadwayWidthAt(junction.splineContainer, junction.splineIndex, t, roadWidth, out Vector3 rightPT, out Vector3 leftPT);

                edges.Add(new Edge(leftPT, rightPT));
                center += rightPT;
                center += leftPT;
            }

            center = center / (2 * edges.Count);

            edges.Sort((x, y) => {

                Vector3 xDir = x.center - center;
                Vector3 yDir = y.center - center;

                float xAngle = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
                float yAngle = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

                if (xAngle > yAngle) return 1;
                else if (xAngle < yAngle) return -1;
                else return 0;
            });

            for (int i = 1; i <= edges.Count; i++)
            {
                Vector3 a = edges[i - 1].left;
                points.Add(a);
                Vector3 b = (i < edges.Count) ? edges[i].right : edges[0].right;
                Vector3 mid = Vector3.Lerp(a, b, 0.5f);

                Vector3 dir = center - mid;
                mid = mid - dir;
                Vector3 c = Vector3.Lerp(mid, center, intersection.curveWeights[i - 1]);

                BezierCurve curve = new BezierCurve(a, c, b);
                for (float t = 0.0f; t < 1.0f; t += 0.1f)
                {
                    Vector3 pos = CurveUtility.EvaluatePosition(curve, t);
                    points.Add(pos);
                }

                points.Add(b);
            }

            return center;
        }

        private static Vector3 GetIntersectionOuterEdges(RoadwayIntersection intersection, float roadWidth, float curveWidth, ref List<Edge> edges, ref List<Vector3> points)
        {
            Vector3 center = Vector3.zero;

            foreach (JunctionInfo junction in intersection.GetJunctions())
            {
                float t = junction.knotIndex == 0 ? 0f : 1f;

                RoadwayHelper.GetRoadwayWidthAt(junction.splineContainer, junction.splineIndex, t, roadWidth + curveWidth, out Vector3 rightPT, out Vector3 leftPT);

                edges.Add(new Edge(leftPT, rightPT));
                center += rightPT;
                center += leftPT;
            }

            center = center / (2 * edges.Count);

            edges.Sort((x, y) => {

                Vector3 xDir = x.center - center;
                Vector3 yDir = y.center - center;

                float xAngle = Vector3.SignedAngle(center.normalized, xDir.normalized, Vector3.up);
                float yAngle = Vector3.SignedAngle(center.normalized, yDir.normalized, Vector3.up);

                if (xAngle > yAngle) return 1;
                else if (xAngle < yAngle) return -1;
                else return 0;
            });

            for (int i = 1; i <= edges.Count; i++)
            {
                Vector3 a = edges[i - 1].left;
                points.Add(a);
                Vector3 b = (i < edges.Count) ? edges[i].right : edges[0].right;
                Vector3 mid = Vector3.Lerp(a, b, 0.5f);

                Vector3 dir = center - mid;
                mid = mid - dir;
                Vector3 c = Vector3.Lerp(mid, center, intersection.curveWeights[i - 1]);

                BezierCurve curve = new BezierCurve(a, c, b);
                for (float t = 0.0f; t < 1.0f; t += 0.1f)
                {
                    Vector3 pos = CurveUtility.EvaluatePosition(curve, t);
                    points.Add(pos);
                }

                points.Add(b);
            }

            return center;
        }

        private static void GenerateIntersection(Vector3 center, List<Vector3> points, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            int vOffset = vertices.Count;

            for (int j = 1; j <= points.Count; j++)
            {
                vertices.Add(center);
                vertices.Add(points[j - 1]);

                if (j == points.Count)
                {
                    vertices.Add(points[0]);
                }
                else
                {
                    vertices.Add(points[j]);
                }

                triangles.Add(vOffset + ((j - 1) * 3) + 0);
                triangles.Add(vOffset + ((j - 1) * 3) + 1);
                triangles.Add(vOffset + ((j - 1) * 3) + 2);
            }
        }

        private static void GenerateIntersectionCurve(RoadwayIntersection intersection, Vector3 center, List<Vector3> pointsInner, List<Vector3> pointsOuter, float roadWidth, float curveHeight, float curveWidth, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            if (curveHeight < 0.001f || curveWidth < 0.001f) return;

            int vOffset = vertices.Count;

            for (int j = 1; j <= pointsInner.Count; j++)
            {
                Vector3 p1 = pointsInner[j - 1];
                Vector3 p2 = pointsInner[j - 1] + Vector3.up * curveHeight;
                Vector3 p3 = pointsOuter[j - 1] + Vector3.up * curveHeight;
                Vector3 p4;
                Vector3 p5;
                Vector3 p6;
                if (j == pointsInner.Count)
                {
                    p4 = pointsInner[0];
                    p5 = pointsInner[0] + Vector3.up * curveHeight;
                    p6 = pointsOuter[0] + Vector3.up * curveHeight; ;
                }
                else
                {
                    p4 = pointsInner[j];
                    p5 = pointsInner[j] + Vector3.up * curveHeight;
                    p6 = pointsOuter[j] + Vector3.up * curveHeight;
                }

                if (intersection.HasJunction(p1, p4, roadWidth)) continue;

                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                vertices.Add(p4);
                vertices.Add(p5);
                vertices.Add(p6);

                // Curve Side
                triangles.Add(vOffset + 0);
                triangles.Add(vOffset + 1);
                triangles.Add(vOffset + 3);

                triangles.Add(vOffset + 3);
                triangles.Add(vOffset + 1);
                triangles.Add(vOffset + 4);

                // Curve Top
                triangles.Add(vOffset + 2);
                triangles.Add(vOffset + 4);
                triangles.Add(vOffset + 1);

                triangles.Add(vOffset + 4);
                triangles.Add(vOffset + 2);
                triangles.Add(vOffset + 5);

                vOffset = vertices.Count;
            }
        }

        private static void CleanMesh(ref List<Vector3> vertices, ref List<int> triangles)
        {
            Dictionary<Vector3, int>  vertexMap = new Dictionary<Vector3, int>();

            List <Vector3> uniqueVerts = new List<Vector3>();
            int[] remapped = new int[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v = vertices[i];
                if (vertexMap.TryGetValue(v, out int existingIndex))
                {
                    remapped[i] = existingIndex;
                }
                else
                {
                    int newIndex = uniqueVerts.Count;
                    uniqueVerts.Add(v);
                    vertexMap[v] = newIndex;
                    remapped[i] = newIndex;
                }
            }

            int[] outTriangles = new int[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                outTriangles[i] = remapped[triangles[i]];
            }

            vertices = uniqueVerts;
            triangles = outTriangles.ToList();
        }

        public static void GenerateIntersectionMesh(RoadwayIntersection intersection, float roadWidth, float curveWidth, float curveHeight)
        {
            GameObject roadwayGameObject = new GameObject("intersection");
            roadwayGameObject.transform.parent = parent;
            MeshRenderer mr = roadwayGameObject.AddComponent<MeshRenderer>();
            MeshFilter mf = roadwayGameObject.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> triangles = new List<int>();

            List<Edge> edgesInner = new List<Edge>();
            List<Vector3> pointsInner = new List<Vector3>();
            List<Edge> edgesOuter = new List<Edge>();
            List<Vector3> pointsOuter= new List<Vector3>();


            Vector3 center = GetIntersectionInnerEdges(intersection, roadWidth, ref edgesInner, ref pointsInner);
            GetIntersectionOuterEdges(intersection, roadWidth, curveWidth, ref edgesOuter, ref pointsOuter);

            GenerateIntersection(center, pointsInner, ref vertices, ref triangles, ref uvs);
            GenerateIntersectionCurve(intersection, center, pointsInner, pointsOuter, roadWidth, curveHeight, curveWidth, ref vertices, ref triangles, ref uvs);

            CleanMesh(ref vertices, ref triangles);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mr.material = mat;
            mf.mesh = mesh;

            if (meshes == null) meshes = new List<GameObject>();
            meshes.Add(roadwayGameObject);
        }

        private static void CreateRoad(Roadway roadway, float roadWidth, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            int vOffset = vertices.Count;

            for (int i = 0; i < roadway.segments.Count; i++)
            {
                RoadwayHelper.GetRoadwayWidthAt(roadway.container, roadway.splineIndex, roadway.segments[i], roadWidth, out Vector3 rightPT, out Vector3 leftPT);
                vertices.Add(rightPT);
                vertices.Add(leftPT);
            }

            int idx = 0;
            float uvOffset = 0;
            for (int i = 0; i < roadway.segments.Count - 1; i++)
            {
                idx = vOffset + i * 2;

                triangles.Add(idx);
                triangles.Add(idx + 2);
                triangles.Add(idx + 1);

                triangles.Add(idx + 1);
                triangles.Add(idx + 2);
                triangles.Add(idx + 3);

                Vector3 p1 = vertices[idx];
                Vector3 p3 = vertices[idx + 2];
                float dist = Vector3.Distance(p1, p3);
                float uvDist = uvOffset + dist;
                uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1) });
                uvOffset += dist;
            }

            uvs.AddRange(new List<Vector2> { new Vector2(uvOffset, 0), new Vector2(uvOffset, 1) });

            if (roadway.container.Splines[roadway.splineIndex].Closed)
            {
                idx = (roadway.segments.Count - 1) * 2;

                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(idx);

                triangles.Add(1);
                triangles.Add(idx + 1);
                triangles.Add(idx);
            }
        }

        private static void CreateCurve(Roadway roadway, float roadWidth, float curveHeight, float curveWidth, ref List<Vector3> vertices, ref List<int> triangles, ref List<Vector2> uvs)
        {
            if (curveHeight < 0.001f || curveWidth < 0.001f) return;

            int vOffset = vertices.Count;

            for (int i = 0; i < roadway.segments.Count; i++)
            {
                RoadwayHelper.GetRoadwayWidthAt(roadway.container, roadway.splineIndex, roadway.segments[i], roadWidth, out Vector3 rightPT, out Vector3 leftPT);
                RoadwayHelper.GetRoadwayWidthAt(roadway.container, roadway.splineIndex, roadway.segments[i], roadWidth + curveWidth, out Vector3 curbRightPT, out Vector3 curbLeftPT);

                vertices.Add(rightPT);
                vertices.Add(rightPT + Vector3.up * curveHeight);
                vertices.Add(curbRightPT + Vector3.up * curveHeight);
                vertices.Add(leftPT);
                vertices.Add(leftPT + Vector3.up * curveHeight);
                vertices.Add(curbLeftPT + Vector3.up * curveHeight);

                // TODO: Map UVs
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 0));
                uvs.Add(new Vector2(0, 0));
            }

            int idx = 0;
            for (int i = 0; i < roadway.segments.Count - 1; i++)
            {
                idx = vOffset + i * 6;

                // Right Curb Side
                triangles.Add(idx + 6);
                triangles.Add(idx);
                triangles.Add(idx + 1);

                triangles.Add(idx + 6);
                triangles.Add(idx + 1);
                triangles.Add(idx + 7);

                // Right Curb
                triangles.Add(idx + 2);
                triangles.Add(idx + 7);
                triangles.Add(idx + 1);

                triangles.Add(idx + 2);
                triangles.Add(idx + 8);
                triangles.Add(idx + 7);

                // Left Curb Side
                triangles.Add(idx + 3);
                triangles.Add(idx + 9);
                triangles.Add(idx + 4);

                triangles.Add(idx + 4);
                triangles.Add(idx + 9);
                triangles.Add(idx + 10);

                // Left Curb
                triangles.Add(idx + 10);
                triangles.Add(idx + 5);
                triangles.Add(idx + 4);

                triangles.Add(idx + 11);
                triangles.Add(idx + 5);
                triangles.Add(idx + 10);
            }
        }

        public static void GenerateRoadMesh(Roadway roadway, float roadWidth, float curveWidth, float curveHeight)
        {
            if (roadway.segments.Count == 0) return;

            GameObject roadwayGameObject = new GameObject("Road");
            roadwayGameObject.transform.parent = parent;
            MeshRenderer mr = roadwayGameObject.AddComponent<MeshRenderer>();
            MeshFilter mf = roadwayGameObject.AddComponent<MeshFilter>();

            Mesh mesh = new Mesh();
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            CreateRoad(roadway, roadWidth, ref vertices, ref triangles, ref uvs);
            CreateCurve(roadway, roadWidth, curveHeight, curveWidth, ref vertices, ref triangles, ref uvs);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mr.material = mat;
            mf.mesh = mesh;

            if (meshes == null) meshes = new List<GameObject>();
            meshes.Add(roadwayGameObject);
        }
    }
}

#endif
