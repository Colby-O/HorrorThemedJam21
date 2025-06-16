using UnityEngine;
using UnityEngine.Events;

namespace HTJ21
{
    public interface IClickable
    {
        public bool CanClick { get; set; }
        public UnityAction OnClick { get; set; }
        public UnityAction OnHoverEnter { get; set; }
        public UnityAction OnHoverExit { get; set; }
    }
}
