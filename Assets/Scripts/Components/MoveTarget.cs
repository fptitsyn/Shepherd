using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct MoveTarget : IComponentData
    {
        public float3 Position;
    }
}