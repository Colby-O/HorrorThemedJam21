using UnityEngine;
using UnityEngine.Serialization;

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
        public float CrouchHeight = 1.2f;
        public float CrouchSpeedMul = 0.5f;

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
        [FormerlySerializedAs("_parkingSpeed")] public float ParkingSpeed = 0.2f;
        public float SteeringWheelAngleScale = 4;
        public float WheelTurnSpeed = 2;
        public float MaxTurnAngle = 35;
        public float MaxSpeedTurnAngle = 10;
        public float MaxSpeedTurnAngleSpeed = 50;
    }
}
