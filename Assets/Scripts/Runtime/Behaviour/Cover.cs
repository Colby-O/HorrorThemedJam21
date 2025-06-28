using PlazmaGames.Attribute;
using UnityEngine;

namespace HTJ21
{
    [System.Serializable]
    public class CoverState
    {
        public static int instanceCount;

        public bool needToCrouch;
        public int id = -1;

        public CoverState(bool needToCrouch)
        {
            this.needToCrouch = needToCrouch;
            id = instanceCount++;
        }
    }

    public class Cover : MonoBehaviour
    {
        [SerializeField] private bool _needToCrouch = false;

        [SerializeField, ReadOnly] private CoverState _state;

        public CoverState GetState() => _state;

        public void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.SetHiddenState(_state);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out PlayerController player))
            {
                player.RemoveHiddenState(_state);
            }
        }

        private void Awake()
        {
            _state = new CoverState(_needToCrouch);
        }
    }
}
