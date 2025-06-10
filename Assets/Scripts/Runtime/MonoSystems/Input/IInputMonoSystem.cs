using UnityEngine;
using PlazmaGames.Core.MonoSystem;
using UnityEngine.Events;

namespace HTJ21
{
    public interface IInputMonoSystem : IMonoSystem
    {
        public Vector2 RawMovement { get; }
        public Vector2 RawLook { get; }
        public bool ReversePressed();
        public bool InteractPressed();
        public bool LightPressed();

        public UnityEvent InteractionCallback { get; }
    }
}
