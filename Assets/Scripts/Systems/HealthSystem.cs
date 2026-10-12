using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Systems
{
    [BurstCompile]
    public partial struct HealthSystem : ISystem
    {
        private EntityQuery _healthQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _healthQuery = SystemAPI.QueryBuilder()
                .WithAll<Health>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

            HealthJob job = new HealthJob
            {
                CommandBuffer = commandBuffer.AsParallelWriter()
            };

            state.Dependency = job.ScheduleParallel(_healthQuery, state.Dependency);

            state.Dependency.Complete();

            commandBuffer.Playback(state.EntityManager);
            commandBuffer.Dispose();
        }

        [BurstCompile]
        public partial struct HealthJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            [BurstCompile]
            public void Execute([ChunkIndexInQuery] int chunkIndexInQuery, Entity entity, in Health health)
            {
                if (health.Value > 0f)
                {
                    return;
                }

                CommandBuffer.DestroyEntity(chunkIndexInQuery, entity);
            }
        }
    }
}