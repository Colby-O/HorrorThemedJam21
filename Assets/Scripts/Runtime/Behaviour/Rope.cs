using System.Collections.Generic;
using System.Linq;
using PlazmaGames.Attribute;
using UnityEngine;
using UnityEngine.Serialization;

namespace HTJ21
{
    public class Rope : MonoBehaviour
    {
        struct Segment
        {
            public Vector3 cur, prev;

            public Segment(Vector3 pos)
            {
                cur = pos;
                prev = pos;
            }
        }
        
        [SerializeField] private Transform _attachment1;
        [SerializeField] private Transform _attachment2;
        [SerializeField] private int _segmentCount = 35;
        [SerializeField] private float _gravity = -1.5f;
        [SerializeField] private int _constrainAmount = 25;
        [SerializeField] private float _ropeRadius = 0.1f;
        
        private float SegmentLength => Vector3.Distance(_attachment1.position, _attachment1.position) / _segmentCount;

        private List<Segment> _segments = new List<Segment>();

        private LineRenderer _lr;
        
		[InspectorButton("Start")] public bool reset = false;


        void Start()
        {
            _lr = GetComponent<LineRenderer>();

            Vector3 ropeStartPoint = _attachment1.position;
            Vector3 dir = (_attachment2.position - _attachment1.position).normalized;
            float seglen = SegmentLength;

            for (int i = 0; i < _segmentCount; i++)
            {
                _segments.Add(new Segment(ropeStartPoint));
                ropeStartPoint += dir * seglen;
            }
        }

        private void Update()
        {
            DrawRope();
        }

        private void FixedUpdate()
        {
            Simulate();
            
            for (int i = 1; i < _segmentCount - 1; i++)
            {
                Segment seg = _segments[i];
                Collider[] hits = Physics.OverlapSphere(seg.cur, _ropeRadius);

                foreach (Collider hit in hits)
                {
                    if (hit.transform == _attachment1 || hit.transform == _attachment2)
                        continue;

                    Vector3 closest = hit.ClosestPoint(seg.cur);
                    Vector3 offset = seg.cur - closest;
                    float penetration = _ropeRadius - offset.magnitude;

                    if (penetration > 0f)
                    {
                        seg.cur += offset.normalized * penetration;
                        Debug.Log(penetration);
                    }
                }

                _segments[i] = seg;
            }
        }

        private void DrawRope()
        {
            Vector3[] ropePositions = new Vector3[_segmentCount];
            for (int i = 0; i < _segmentCount; i++)
            {
                ropePositions[i] = _segments[i].cur;
            }
            _lr.positionCount = ropePositions.Length;
            _lr.SetPositions(ropePositions);
        }

        private void Simulate()
        {
            Vector3 forceGravity = new Vector3(0f, _gravity, 0);

            for (int i = 1; i < _segmentCount; i++)
            {
                Segment firstSegment = _segments[i];
                Vector3 velocity = firstSegment.cur - firstSegment.prev;
                firstSegment.prev = firstSegment.cur;
                firstSegment.cur += velocity;
                firstSegment.cur += forceGravity * Time.fixedDeltaTime;
                _segments[i] = firstSegment;
            }

            for (int i = 0; i < _constrainAmount; i++)
            {
                this.ApplyConstraint();
            }
        }
        
        private void ApplyConstraint()
        {
            Segment firstSegment = _segments[0];
            firstSegment.cur = _attachment1.position;
            _segments[0] = firstSegment;

            Segment lastSegment = _segments[^1];
            lastSegment.cur = _attachment2.position;
            _segments[^1] = lastSegment;

            for (int i = 0; i < _segmentCount - 1; i++)
            {
                Segment firstSeg = _segments[i];
                Segment secondSeg = _segments[i + 1];

                float dist = (firstSeg.cur - secondSeg.cur).magnitude;
                float error = Mathf.Abs(dist - this.SegmentLength);
                Vector3 changeDir = Vector3.zero;

                if (dist > SegmentLength)
                {
                    changeDir = (firstSeg.cur - secondSeg.cur).normalized;
                } else if (dist < SegmentLength)
                {
                    changeDir = (secondSeg.cur - firstSeg.cur).normalized;
                }
                Vector3 changeAmount = changeDir * error;

                if (i != 0)
                {
                    firstSeg.cur -= changeAmount * 0.5f;
                    _segments[i] = firstSeg;
                    secondSeg.cur += changeAmount * 0.5f;
                    _segments[i + 1] = secondSeg;
                }
                else
                {
                    secondSeg.cur += changeAmount;
                    _segments[i + 1] = secondSeg;
                }
            }
            
            
        }
    }
}
