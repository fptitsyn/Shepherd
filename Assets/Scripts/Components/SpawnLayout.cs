using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct SpawnLayoutBlob
    {
        public BlobArray<float3> Positions;
    }

    public struct SpawnLayout : IComponentData
    {
        public BlobAssetReference<SpawnLayoutBlob> BlobReference;
    }
}