using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    public class HitchInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private MeshRenderer[] _outlineMR;
        
        private bool _hasOutline = false;

        private SpringJoint _carJoint;
        private GameObject _ropeItem;
        private Rope _rope;

        private void Start()
        {
            _carJoint = HTJ21GameManager.Car.GetComponent<SpringJoint>();
        }

        public void AttachRope(Rope rope, GameObject item, Rigidbody to)
        {
            _rope = rope;
            _rope.Attach(transform, to.transform);
            _ropeItem = item;
            HTJ21GameManager.Player.GetComponent<Inspector>().EndInspect();
            _ropeItem.SetActive(false);
            _carJoint.connectedBody = to;
            _carJoint.massScale = 1;
        }

        public void AddOutline()
        {
            _hasOutline = true;
            foreach (MeshRenderer mr in _outlineMR)
            {
                Material[] mats = mr.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].SetInt("Boolean_8BBF99CD", 0);
                }

                mr.materials = mats;
            }
        }

        public void RemoveOutline()
        {
            _hasOutline = false;
            foreach (MeshRenderer mr in _outlineMR)
            {
                Material[] mats = mr.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].SetInt("Boolean_8BBF99CD", 1);
                }

                mr.materials = mats;
            }
        }

        public bool IsInteractable()
        {
            return _carJoint.connectedBody;
        }

        public bool Interact(Interactor interactor)
        {
            if (_carJoint.connectedBody)
            {
                _carJoint.connectedBody = null;
                _carJoint.massScale = 1e-5f;
                _ropeItem.SetActive(true);
                _ropeItem.transform.position = transform.position + -HTJ21GameManager.Car.transform.forward * 0.4f;
                Destroy(_rope.gameObject);
            }
            return true;
        }

        public void EndInteraction()
        {

        }

        public string GetHint()
        {
            return $"Click 'E' To Detach Rope.";
        }
    }
}
