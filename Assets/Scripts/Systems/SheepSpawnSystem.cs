using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    public partial struct SheepSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SheepSpawner>();
        }

        public void OnUpdate(ref SystemState state)
        {
            SheepSpawner spawner = SystemAPI.GetSingleton<SheepSpawner>();

            NativeArray<Entity> sheep = new NativeArray<Entity>(spawner.Count, Allocator.Temp);

            state.EntityManager.Instantiate(spawner.Prefab, sheep);

            for (int i = 0; i < sheep.Length; i++)
            {
                int row = i / spawner.RowSize;
                int column = i % spawner.RowSize;

                float width = spawner.RowSize * spawner.Spacing;
                float3 position = new float3(
                    column * spawner.Spacing - width * 0.5f,
                    1f,
                    row * spawner.Spacing - width * 0.5f);

                LocalTransform transform = state.EntityManager.GetComponentData<LocalTransform>(sheep[i]);

                transform.Position = position;

                state.EntityManager.SetComponentData(sheep[i], transform);
            }

            sheep.Dispose();

            Entity spawnerEntity = SystemAPI.GetSingletonEntity<SheepSpawner>();

            state.EntityManager.DestroyEntity(spawnerEntity);
        }
    }
}