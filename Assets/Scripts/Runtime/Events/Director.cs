using UnityEngine;

namespace HTJ21
{
    public abstract class Director : MonoBehaviour
    {
        [SerializeField] private Act _act;

        public Act GetAct() => _act;

        public abstract void OnActInit();
        public abstract void OnActStart();
        public abstract void OnActUpdate();
        public abstract void OnActEnd();
    }
}
