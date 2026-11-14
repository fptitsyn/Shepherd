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
                .WithAll<Sheep, LocalTransform, MoveTarget>()
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

            public void Execute(Entity entity, ref Danger danger, in MoveTarget target, in LocalTransform transform)
            {
                float3 position = transform.Position;
                float3 toGoal = target.Position - position;
                toGoal.y = 0f;

                float goalDistanceSq = math.lengthsq(toGoal);

                danger.SteeringTarget = target.Position;

                if (goalDistanceSq < 0.01f)
                {
                    danger.IsFleeing = false;
                    danger.AvoidedWolf = Entity.Null;
                    danger.AvoidanceSide = 0f;
                    return;
                }

                float radius = math.max(0.1f, danger.AvoidanceRadius);

                NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
                CollisionWorld collisionWorld = World.CollisionWorld;

                collisionWorld.OverlapSphere(position, danger.DetectionRadius,
                    ref hits, CollisionFilter.Default, QueryInteraction.Default);

                Entity blockingWolf = Entity.Null;
                float3 wolfPosition = float3.zero;
                float nearestDistanceSq = float.MaxValue;

                for (int i = 0; i < hits.Length; i++)
                {
                    DistanceHit hit = hits[i];

                    if (!Wolves.HasComponent(hit.Entity))
                    {
                        continue;
                    }

                    float3 candidate = collisionWorld.Bodies[hit.RigidBodyIndex].WorldFromBody.pos;
                    candidate.y = position.y;

                    float3 toWolf = candidate - position;
                    float projection = math.saturate(math.dot(toWolf, toGoal) / goalDistanceSq);

                    float3 closestPoint = position + toGoal * projection;

                    if (math.distancesq(closestPoint, candidate) >= radius * radius)
                    {
                        continue;
                    }

                    float distanceSq = math.lengthsq(toWolf);

                    if (distanceSq < nearestDistanceSq)
                    {
                        nearestDistanceSq = distanceSq;
                        blockingWolf = hit.Entity;
                        wolfPosition = candidate;
                    }
                }

                hits.Dispose();

                if (blockingWolf == Entity.Null)
                {
                    danger.IsFleeing = false;
                    danger.AvoidedWolf = Entity.Null;
                    danger.AvoidanceSide = 0f;
                    return;
                }

                float3 radial = position - wolfPosition;
                radial.y = 0f;

                float distance = math.length(radial);

                if (distance < 0.001f)
                {
                    radial = new float3(1f, 0f, 0f);
                }
                else
                {
                    radial /= distance;
                }

                float3 tangent = new float3(-radial.z, 0f, radial.x);

                if (!danger.IsFleeing || danger.AvoidedWolf != blockingWolf)
                {
                    float preference = math.dot(tangent, toGoal);

                    if (math.abs(preference) < 0.001f)
                    {
                        danger.AvoidanceSide = (entity.Index & 1) == 0 ? 1f : -1f;
                    }
                    else
                    {
                        danger.AvoidanceSide = preference > 0f ? 1f : -1f;
                    }
                }

                float desiredRadius = radius + 0.5f;
                float correction = math.clamp((desiredRadius - distance) * 1.5f, -0.8f, 2f);

                float3 direction = tangent * danger.AvoidanceSide + radial * correction;
                direction = math.normalizesafe(direction, radial);

                danger.SteeringTarget = position + direction * math.max(1f, danger.EscapeDistance);
                danger.IsFleeing = true;
                danger.AvoidedWolf = blockingWolf;
            }
        }
    }
}