using UnityEngine;

namespace HTJ21
{
    [CreateAssetMenu(fileName = "DefaultPlayerMovementSettings", menuName = "Player/MovementSettings")]
    public class PlayerSettings : ScriptableObject
    {
        [Header("Movement")]
        public float Speed;
        public float WalkingForwardSpeed = 1f;
        public float WalkingBackwardSpeed = 0.5f;
        public float WalkingStrideSpeed = 1f;
        public float MovementSmoothing = 0.5f;
        public float GravityMultiplier = 1.0f;

        [Header("Look")]
        public Vector2 Sensitivity = Vector2.one;
        public Vector2 YLookLimit;
        public bool InvertLookY = false;
        public bool InvertLookX = false;

        [Header("Car")]
        public Vector2 CarYLookLimit;
        public Vector2 CarXLookLimit;
        public float HeadBobbleSpeed = 5;
        public float HeadBobbleMagnitude = 0.16f;
        public float RpmScale = 3;
    }
}
