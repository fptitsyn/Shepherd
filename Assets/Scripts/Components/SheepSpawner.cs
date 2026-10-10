using Unity.Entities;

namespace Components
{
    public struct SheepSpawner : IComponentData
    {
        public int Count;
        public int RowSize;
        public float Spacing;
    }
}