using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.SocialPlatforms;

namespace Systems
{
    [BurstCompile]
    public partial struct MovementSystem : ISystem
    {
        private EntityQuery _sheepMovementQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _sheepMovementQuery = SystemAPI.QueryBuilder()
                .WithAll<Sheep, MoveTarget, MoveSpeed>()
                .WithAllRW<LocalTransform>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            MovementJob job = new MovementJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(_sheepMovementQuery, state.Dependency);
        }

        [BurstCompile]
        public partial struct MovementJob : IJobEntity
        {
            public float DeltaTime;

            [BurstCompile]
            public void Execute(ref LocalTransform transform, in MoveTarget target, in MoveSpeed speed)
            {
                float3 direction = target.Position - transform.Position;
                float distance = math.length(direction);

                if (distance < 0.1f)
                {
                    return;
                }

                direction = math.normalize(direction);
                transform.Position += direction * speed.Value * DeltaTime;
            }
        }
    }
}