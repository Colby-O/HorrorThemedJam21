using PlazmaGames.Attribute;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.XR;

namespace HTJ21
{
    public class PortalObject : MonoBehaviour
    {
        [ReadOnly] public Vector3 PreviousOffsetFromPortal;

        [SerializeField, ReadOnly] private CharacterController _controller;

        public UnityEvent<Portal, Portal> OnPortalEnter { get; set; }

        public void Teleport(Vector3 pos, Quaternion rot)
        {
            if (_controller) _controller.enabled = false;
            transform.position = pos;
            transform.rotation = rot;
            if (_controller) _controller.enabled = true;
        }

        private void Awake()
        {
            OnPortalEnter = new UnityEvent<Portal, Portal>();
            _controller = GetComponent<CharacterController>();
        }
    }
}
