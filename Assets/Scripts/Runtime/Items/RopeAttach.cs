using System;
using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    public class RopeAttach : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Rigidbody _attachedTo;

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
                            _attachedTo = null;
                        }
                    }
                    else
                    {
                        _attachedTo = rig;
                    }
                }
            }
            if (other.gameObject.CompareTag("CarHitch"))
            {
                if (_attachedTo)
                {
                    Debug.Log("HITCH TO CAR");
                    other.gameObject.GetComponent<HitchInteractable>().AttachRope(gameObject, _attachedTo);
                    _attachedTo = null;
                }
            }
        }
        
        private void OnCollisionEnter(Collision other)
        {
        }
    }

}
