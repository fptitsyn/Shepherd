using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct Danger : IComponentData
    {
        public float DetectionRadius;
        public float EscapeDistance;
        public float AvoidanceRadius;

        public bool IsFleeing;
        public float3 SteeringTarget;

        public Entity AvoidedWolf;
        public float AvoidanceSide;
    }
}