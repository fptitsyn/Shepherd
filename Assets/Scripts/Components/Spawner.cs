using Unity.Entities;

namespace Components
{
    public struct Spawner : IComponentData
    {
        public int Count;
        public int RowSize;
        public float Spacing;
    }
}