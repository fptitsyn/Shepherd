using Unity.Entities;

namespace Components
{
    public struct Danger : IComponentData
    {
        public float DetectionRadius;
        public float EscapeDistance;
        public bool IsFleeing;
    }
}