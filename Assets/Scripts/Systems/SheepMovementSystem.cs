using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct SheepMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (transform, target, speed) in SystemAPI.Query<
                     RefRW<LocalTransform>,
                     RefRO<MoveTarget>,
                     RefRO<MoveSpeed>>()
                         .WithAll<Sheep>())
            {
                float3 direction = target.ValueRO.Position - transform.ValueRO.Position;
                float distance = math.length(direction);

                if (distance < 0.1f)
                {
                    continue;
                }

                direction = math.normalize(direction);

                transform.ValueRW.Position += direction * speed.ValueRO.Value * deltaTime;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}