using System.Collections.Generic;
using PlazmaGames.Attribute;
using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public class CultAnimator : MonoBehaviour
    {
        class Rig
        {
            public List<Transform> parts = new();
        }

        private Rig _rig;

        private List<Rig> _frames = new();
        
        [SerializeField] private Transform _rigTransform;
        [SerializeField] private Transform _animation;
        [SerializeField] private float _speed = 2;

        [SerializeField, ReadOnly] private float _t = 0;
        [SerializeField, ReadOnly] private int _dir = 1;
        [SerializeField] private bool _running = true;

        public UnityEvent OnFinish;
        public UnityEvent OnHalfFinish;

        public void Play() => _running = true;
        public void Stop() => _running = true;

        public void Reset()
        {
            Stop();
            _t = 0;
            _dir = 1;
        }

        private void Update()
        {
            if (!_running || HTJ21GameManager.IsPaused) return;

            _t += Time.deltaTime / _speed * _dir;
            if (_t > 1) _t = 1;
            if (_t < 0) _t = 0;
            float lt = (_t % (1.0f / _frames.Count)) * _frames.Count;

            int idx = Mathf.FloorToInt(_t * _frames.Count);

            if (idx == _frames.Count - 1 && _dir == 1)
            {
                _dir = -1;
                _t = 1;
                lt = 1;
                OnHalfFinish.Invoke();
            }

            if (idx == 0 && _dir == -1)
            {
                _dir = 1;
                _t = 0;
                lt = 0;
                _running = false;
                OnFinish.Invoke();
            }

            if (_dir < 0) lt = 1.0f - lt;
            for (int i = 0; i < _rig.parts.Count; i++)
            {
                Transform from = _frames[idx].parts[i];
                Transform to = _frames[idx + _dir].parts[i];
                _rig.parts[i].localPosition = Vector3.Lerp(from.localPosition, to.localPosition, lt);
                _rig.parts[i].localRotation = Quaternion.Lerp(from.localRotation, to.localRotation, lt);
            }
        }
        
        private void Start()
        {
            _rig = LoadRig(_rigTransform);

            foreach (Transform frame in _animation)
            {
                _frames.Add(LoadRig(frame.GetChild(0)));
            }
        }

        private Rig LoadRig(Transform rt)
        {
            Rig r = new Rig();
            r.parts.Add(rt.Find("Head").Find("Head_target"));
            r.parts.Add(rt.Find("Spine").Find("Spine_target"));
            r.parts.Add(rt.Find("LHand").Find("LHand_target"));
            r.parts.Add(rt.Find("RHand").Find("RHand_target"));
            r.parts.Add(rt.Find("LFoot").Find("LFoot_target"));
            r.parts.Add(rt.Find("RFoot").Find("RFoot_target"));
            
            return r;
        }
    }
}
