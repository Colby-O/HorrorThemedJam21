using UnityEngine;
using PlazmaGames.Core.MonoSystem;
using UnityEngine.Events;

namespace HTJ21
{
    public interface IInputMonoSystem : IMonoSystem
    {
        public Vector2 RawMovement { get; }
        public Vector2 RawLook { get; }
        public bool InteractPressed();
        public bool LightPressed();
        public bool Crouched();
        public bool JustCrouched();
        public bool JustUncrouched();

        public UnityEvent InteractionCallback { get; }
        public UnityEvent SkipCallback { get; }
        public UnityEvent LightCallback { get; }
        public UnityEvent RCallback { get; }
    }
}
