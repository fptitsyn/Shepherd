using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Authoring
{
    public class SpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private int count = 1000;
        [SerializeField] private int rowSize = 40;
        [SerializeField] private float spacing = 2f;

        private class Baker : Baker<SpawnerAuthoring>
        {
            public override void Bake(SpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Spawner
                {
                    Count = authoring.count,
                    RowSize = authoring.rowSize,
                    Spacing = authoring.spacing,
                });

                BlobBuilder blobBuilder =
                    new BlobBuilder(Allocator.Temp);

                ref SpawnLayoutBlob root =
                    ref blobBuilder.ConstructRoot<SpawnLayoutBlob>();

                BlobBuilderArray<float3> positions =
                    blobBuilder.Allocate(
                        ref root.Positions,
                        authoring.count);

                for (int i = 0; i < authoring.count; i++)
                {
                    int row = i / authoring.rowSize;
                    int column = i % authoring.rowSize;

                    positions[i] = new float3(
                        column * authoring.spacing,
                        0f,
                        row * authoring.spacing);
                }

                BlobAssetReference<SpawnLayoutBlob> blobReference =
                    blobBuilder.CreateBlobAssetReference<SpawnLayoutBlob>(Allocator.Persistent);

                blobBuilder.Dispose();

                AddBlobAsset(ref blobReference, out _);

                AddComponent(entity, new SpawnLayout
                {
                    Value = blobReference
                });
            }
        }
    }
}