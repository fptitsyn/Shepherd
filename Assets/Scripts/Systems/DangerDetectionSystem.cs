using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [UpdateBefore(typeof(TriggerSystem))]
    public partial struct DangerDetectionSystem : ISystem
    {
        private EntityQuery _sheepDangerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _sheepDangerQuery = SystemAPI.QueryBuilder()
                .WithAll<Sheep, LocalTransform>()
                .WithAllRW<MoveTarget, Danger>()
                .Build();

            state.RequireForUpdate(_sheepDangerQuery);
            state.RequireForUpdate<PhysicsWorldSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SheepDangerDetectionJob job = new SheepDangerDetectionJob
            {
                World = SystemAPI.GetSingleton<PhysicsWorldSingleton>(),
                Wolves = SystemAPI.GetComponentLookup<Wolf>(true)
            };

            state.Dependency = job.ScheduleParallel(_sheepDangerQuery, state.Dependency);
        }

        [BurstCompile]
        public partial struct SheepDangerDetectionJob : IJobEntity
        {
            [ReadOnly] public PhysicsWorldSingleton World;
            [ReadOnly] public ComponentLookup<Wolf> Wolves;

            public void Execute(Entity entity, ref MoveTarget target, ref Danger danger, in LocalTransform transform)
            {
                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
                CollisionWorld collisionWorld = World.CollisionWorld;

                collisionWorld.OverlapSphere(transform.Position, danger.DetectionRadius,
                    ref hits, CollisionFilter.Default, QueryInteraction.Default);

                bool foundWolf = false;
                float nearestDistanceSq = float.MaxValue;
                float3 nearestWolfPosition = float3.zero;

                for (int i = 0; i < hits.Length; i++)
                {
                    DistanceHit hit = hits[i];

                    if (!Wolves.HasComponent(hit.Entity))
                    {
                        continue;
                    }

                    float3 wolfPosition = collisionWorld.Bodies[hit.RigidBodyIndex].WorldFromBody.pos;
                    float distanceSq = math.distancesq(transform.Position, wolfPosition);

                    if (distanceSq < nearestDistanceSq)
                    {
                        nearestDistanceSq = distanceSq;
                        nearestWolfPosition = wolfPosition;
                        foundWolf = true;
                    }
                }

                hits.Dispose();

                if (!foundWolf)
                {
                    if (danger.IsFleeing)
                    {
                        target.Position = transform.Position;
                    }

                    danger.IsFleeing = false;
                    return;
                }

                float3 direction = transform.Position - nearestWolfPosition;
                direction.y = 0f;

                if (math.lengthsq(direction) < 0.0001f)
                {
                    uint seed = math.hash(new uint2((uint)entity.Index, (uint)entity.Version));
                    Random random = new Random(math.max(1u, seed));
                    float angle = random.NextFloat(0f, 2f * math.PI);

                    direction = new float3(math.cos(angle), 0f, math.sin(angle));
                }

                direction = math.normalize(direction);

                target.Position = transform.Position + direction * danger.EscapeDistance;
                danger.IsFleeing = true;
            }
        }
    }
}