#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;
using PlazmaGames.Attribute;

namespace HTJ21
{
    [Serializable]
    public struct JunctionInfo
    {
        public int splineIndex;
        public int knotIndex;
        public SplineContainer splineContainer;
        public SelectableKnot knot;

        public JunctionInfo(int splineIndex, int knotIndex, SplineContainer splineContainer, SelectableKnot knot) 
        { 
            this.splineIndex = splineIndex;
            this.knotIndex = knotIndex;
            this.splineContainer = splineContainer;
            this.knot = knot;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (other is JunctionInfo otherJunction)
            {
                return otherJunction.splineContainer == this.splineContainer && otherJunction.splineIndex == this.splineIndex && otherJunction.knotIndex == this.knotIndex;
            }
            return false;
        }
    }

    [Serializable]
    public class RoadwayIntersection
    {
        [SerializeField, ReadOnly] private List<JunctionInfo> _junctions;
        [SerializeField] public List<float> curveWeights;

        public void AddJunction(int splineIndex, int knotIndex, SplineContainer splineContainer, SelectableKnot knot, List<float> curveWeights)
        {
            this.curveWeights = curveWeights;
            _junctions ??= new List<JunctionInfo>();
            _junctions.Add(new JunctionInfo(splineIndex, knotIndex, splineContainer, knot));
        }

        public List<JunctionInfo> GetJunctions()
        {
            return _junctions;
        }

        public bool HasJunction(Vector3 leftPT, Vector3 rightPT, float roadWidth)
        {
            foreach (JunctionInfo junction in _junctions)
            {
                float t = junction.knotIndex == 0 ? 0f : 1f;

                RoadwayHelper.GetRoadwayWidthAt(junction.splineContainer, junction.splineIndex, t, roadWidth, out Vector3 p1, out Vector3 p2);

                if ((p1 == leftPT && p2 == rightPT) || (p1 == rightPT && p2 == leftPT)) return true;
            }

            return false;
        }

        public bool HasJunction(SelectableKnot knot)
        {
            return HasJunction(new JunctionInfo(
                    knot.SplineInfo.Index,
                    knot.KnotIndex,
                    knot.SplineInfo.Container as SplineContainer,
                    knot
            ));
        }

        public bool HasJunction(JunctionInfo junction)
        {
            for (int i = 0; i < _junctions.Count; i++)
            {
                bool hasJunction = _junctions.Contains(junction);
                if (hasJunction) return true;
            }
            return false;
        }

        public bool HasJunctions(List<SelectableKnot> knots, bool checkIfSame)
        {
            if (knots == null || knots.Count == 0) return false;

            List<JunctionInfo> junctions = new List<JunctionInfo>();

            foreach (SelectableKnot knot in knots)
            {
                junctions.Add(new JunctionInfo(
                    knot.SplineInfo.Index,
                    knot.KnotIndex,
                    knot.SplineInfo.Container as SplineContainer,
                    knot
                ));
            }

            return HasJunctions(junctions, checkIfSame);
        }

        public bool HasJunctions(List<JunctionInfo> junctions, bool checkIfSame)
        {
            if (junctions == null || junctions.Count == 0) return false;

            bool isSame = (junctions != null && _junctions != null && _junctions.Count == junctions.Count) || !checkIfSame;

            for (int i = 0; i < junctions.Count; i++)
            {
                isSame &= _junctions.Contains(junctions[i]);
                if (!isSame) return false;
            }

            return isSame;
        }

        public bool HasAtLeastOneJunction(List<SelectableKnot> knots)
        {
            if (knots == null || knots.Count == 0) return false;

            List<JunctionInfo> junctions = new List<JunctionInfo>();

            foreach (SelectableKnot knot in knots)
            {
                junctions.Add(new JunctionInfo(
                    knot.SplineInfo.Index,
                    knot.KnotIndex,
                    knot.SplineInfo.Container as SplineContainer,
                    knot
                ));
            }

            return HasAtLeastOneJunction(junctions);
        }

        public bool HasAtLeastOneJunction(List<JunctionInfo> junctions)
        {
            if (junctions == null || junctions.Count == 0) return false;

            for (int i = 0; i < junctions.Count; i++)
            {
                bool hasJunction= _junctions.Contains(junctions[i]);
                if (hasJunction) return true;
            }

            return false;
        }

        public override bool Equals(object other)
        {
            if (other == null) return false;
            if (other is RoadwayIntersection otherIntersection) return HasJunctions(otherIntersection._junctions, true);
            return false;
        }
    }
}
#endif
