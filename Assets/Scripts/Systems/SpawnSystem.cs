using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public partial struct SpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Spawner>();
            state.RequireForUpdate<PrefabHolder>();
        }

        public void OnUpdate(ref SystemState state)
        {
            PrefabHolder prefabs = SystemAPI.GetSingleton<PrefabHolder>();
            Entity spawnerEntity = SystemAPI.GetSingletonEntity<Spawner>();
            LocalTransform spawnerTransform = SystemAPI.GetComponent<LocalTransform>(spawnerEntity);
            LocalTransform prefabTransform = SystemAPI.GetComponent<LocalTransform>(prefabs.SheepPrefab);
            SpawnLayout layout = SystemAPI.GetComponent<SpawnLayout>(spawnerEntity);

            ref BlobArray<float3> positions = ref layout.BlobReference.Value.Positions;

            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

            for (int i = 0; i < positions.Length; i++)
            {
                Entity sheep = ecb.Instantiate(prefabs.SheepPrefab);

                float3 position = spawnerTransform.Position + math.rotate(spawnerTransform.Rotation, positions[i]);

                LocalTransform transform = prefabTransform;
                transform.Position = position;

                ecb.SetComponent(sheep, transform);
            }

            ecb.DestroyEntity(spawnerEntity);

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}