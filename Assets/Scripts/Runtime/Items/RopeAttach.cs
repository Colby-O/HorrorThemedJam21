using System;
using PlazmaGames.Attribute;
using PlazmaGames.Audio;
using PlazmaGames.Core;
using UnityEditor;
using UnityEngine;
using AudioType = PlazmaGames.Audio.AudioType;

namespace HTJ21
{
    public class RopeAttach : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Rigidbody _attachedTo = null;

        private GameObject _ropePrefab;
        
        private Rope _rope = null;

        private void Start()
        {
            _ropePrefab = Resources.Load<GameObject>("Prefabs/Rope");
        }

        private void Update()
        {
            if (_attachedTo)
            {
                float distSq = (transform.position - _attachedTo.transform.position).sqrMagnitude;
                if (distSq > MathExt.Square(HTJ21GameManager.Preferences.RopeSnapDistance))
                {
                    _attachedTo = null;
                    GameManager.GetMonoSystem<IAudioMonoSystem>().PlayAudio(HTJ21GameManager.Preferences.RopeSnapSound, AudioType.Sfx, false, true);
                    Destroy(_rope.gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("RopeAttachable"))
            {
                if (other.gameObject.TryGetComponent(out Rigidbody rig))
                {
                    if (_attachedTo)
                    {
                        if (_attachedTo == rig)
                        {
                            Destroy(_rope.gameObject);
                            _attachedTo = null;
                        }
                    }
                    else
                    {
                        _attachedTo = rig;
                        _rope = GameObject.Instantiate(_ropePrefab).GetComponent<Rope>();
                        _rope.Attach(transform, _attachedTo.transform);
                    }
                }
            }
            if (other.gameObject.CompareTag("CarHitch"))
            {
                if (_attachedTo)
                {
                    other.gameObject.GetComponent<HitchInteractable>().AttachRope(_rope, gameObject, _attachedTo);
                    _rope = null;
                    _attachedTo = null;
                }
            }
        }
    }

}
