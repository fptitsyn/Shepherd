using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;

namespace Systems
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(MovementSystem))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    public partial struct RandomTargetSystem : ISystem
    {
        private EntityQuery _randomTargetQuery;
        private float _timer;
        private uint _seed;

        public void OnCreate(ref SystemState state)
        {
            _timer = 0f;
            _seed = (uint)UnityEngine.Random.Range(1, 100000);

            _randomTargetQuery = SystemAPI.QueryBuilder()
                .WithAll<Sheep, Danger>()
                .WithAllRW<MoveTarget>()
                .Build();

            state.RequireForUpdate(_randomTargetQuery);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _timer += SystemAPI.Time.DeltaTime;

            if (_timer < 3f)
            {
                return;
            }

            _timer %= 3f;
            _seed = _seed == uint.MaxValue ? 1 : _seed + 1;

            RandomTargetJob job = new RandomTargetJob
            {
                Seed = _seed,
                Chance = 0.02f,
                TargetOffset = 15f
            };

            state.Dependency = job.ScheduleParallel(_randomTargetQuery, state.Dependency);
        }

        [BurstCompile]
        public partial struct RandomTargetJob : IJobEntity
        {
            public uint Seed;
            public float Chance;
            public float TargetOffset;

            public void Execute(Entity entity, ref MoveTarget target, in Danger danger)
            {
                if (danger.IsFleeing)
                {
                    return;
                }

                uint seed = math.hash(new uint3(Seed, (uint)entity.Index, (uint)entity.Version));
                Random random = new Random(math.max(1u, seed));

                if (random.NextFloat() >= Chance)
                {
                    return;
                }

                float x = random.NextFloat(-TargetOffset, TargetOffset);
                float z = random.NextFloat(-TargetOffset, TargetOffset);

                target.Position += new float3(x, 0f, z);
            }
        }
    }
}