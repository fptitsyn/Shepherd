using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Systems
{
    public partial struct DangerDetectionSystem : ISystem
    {
        private EntityQuery _sheepDangerQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _sheepDangerQuery = SystemAPI.QueryBuilder()
                .WithAll<Sheep, LocalTransform>()
                .WithAllRW<Danger>()
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

            public void Execute(ref Danger danger, in LocalTransform transform)
            {
                float3 position = transform.Position;

                if (danger.IsFleeing)
                {
                    if (math.distancesq(danger.SteeringTarget.xy, position.xy) <= 0.09f)
                    {
                        danger.IsFleeing = false;
                    }

                    return;
                }

                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
                CollisionWorld collisionWorld = World.CollisionWorld;

                CollisionFilter filter = new CollisionFilter
                {
                    BelongsTo = 1 << 6,
                    CollidesWith = (1 << 7),
                    GroupIndex = 0
                };
                collisionWorld.OverlapSphere(position, danger.DetectionRadius, ref hits, filter);

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
                    float distanceSq = math.distancesq(position, wolfPosition);

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
                    return;
                }

                float3 direction = position - nearestWolfPosition;
                direction.y = 0f;

                direction = math.normalizesafe(direction, new float3(0f, 0f, 1f));

                danger.SteeringTarget = position + direction * danger.EscapeDistance;
                danger.IsFleeing = true;
            }
        }
    }
}