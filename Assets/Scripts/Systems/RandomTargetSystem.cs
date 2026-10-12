using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Systems
{
    [BurstCompile]
    public partial struct RandomTargetSystem : ISystem
    {
        private float _timer;
        private Random _random;

        public void OnCreate(ref SystemState state)
        {
            _timer = 0f;
            _random = new Random()
            {
                state = (uint)UnityEngine.Random.Range(1, 100000)
            };
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

            foreach (var target in
                     SystemAPI.Query<RefRW<MoveTarget>>()
                         .WithAll<Sheep>())
            {
                float chance = _random.NextFloat();

                if (chance > 0.02f)
                {
                    continue;
                }
                
                float3 commonTarget = target.ValueRO.Position;

                target.ValueRW.Position = commonTarget + new float3(
                    _random.NextFloat(-15f, 15f),
                    0f,
                    _random.NextFloat(-15f, 15f));
            }
        }
    }
}