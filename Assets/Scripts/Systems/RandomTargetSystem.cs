using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [BurstCompile]
    public partial struct RandomTargetSystem : ISystem
    {
        private EntityQuery _randomTargetQuery;
        private float _timer;
        private Random _random;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _timer = 0f;
            _random = new Random
            {
                state = (uint)UnityEngine.Random.Range(1, 100000)
            };

            _randomTargetQuery = SystemAPI.QueryBuilder()
                .WithAll<Sheep>()
                .WithAllRW<MoveTarget>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            _timer += SystemAPI.Time.DeltaTime;
            if (_timer < 3f)
            {
                return;
            }
            _timer = 0f;

            RandomTargetJob job = new RandomTargetJob
            {
                Chance = 0.02f,
                TargetOffset = 15f,
                Random = _random
            };

            state.Dependency = job.ScheduleParallel(_randomTargetQuery, state.Dependency);
        }

        [BurstCompile]
        public partial struct RandomTargetJob : IJobEntity
        {
            public float Chance;
            public float TargetOffset;
            public Random Random;

            [BurstCompile]
            public void Execute(ref MoveTarget target)
            {
                if (Random.NextFloat() > Chance)
                {
                    return;
                }

                float3 commonTarget = target.Position;

                target.Position = commonTarget + new float3(
                    Random.NextFloat(-TargetOffset, TargetOffset),
                    0f,
                    Random.NextFloat(-TargetOffset, TargetOffset));
            }
        }
    }
}