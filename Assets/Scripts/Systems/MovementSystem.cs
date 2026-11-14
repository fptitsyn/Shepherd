using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct MovementSystem : ISystem
    {
        private EntityQuery _sheepMovementQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _sheepMovementQuery = SystemAPI.QueryBuilder()
                .WithAll<Sheep, MoveTarget, MoveSpeed, LocalTransform>()
                .WithAllRW<PhysicsVelocity, PhysicsMass>()
                .Build();

            state.RequireForUpdate(_sheepMovementQuery);
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
            public void Execute(ref PhysicsVelocity velocity, ref PhysicsMass mass,
                in LocalTransform transform, in MoveTarget target, in MoveSpeed speed)
            {
                mass.InverseInertia = float3.zero;
                velocity.Angular = float3.zero;

                float3 direction = target.Position - transform.Position;
                direction.y = 0f;

                float distance = math.length(direction);
                float3 horizontalVelocity = float3.zero;

                if (distance > 0.1f)
                {
                    float maxSpeed = (distance - 0.1f) / math.max(DeltaTime, 0.0001f);
                    float movementSpeed = math.min(math.max(0f, speed.Value), maxSpeed);

                    horizontalVelocity = direction / distance * movementSpeed;
                }

                velocity.Linear.x = horizontalVelocity.x;
                velocity.Linear.z = horizontalVelocity.z;
            }
        }
    }
}