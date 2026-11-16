using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

namespace Systems
{
    public partial struct TriggerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            WolfTriggerJob job = new WolfTriggerJob
            {
                Wolves = SystemAPI.GetComponentLookup<Wolf>(true),
                SheepLookup = SystemAPI.GetComponentLookup<Sheep>(true),
                HealthLookup = SystemAPI.GetComponentLookup<Health>(),
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }

        [BurstCompile]
        public struct WolfTriggerJob : ITriggerEventsJob
        {
            [ReadOnly] public ComponentLookup<Wolf> Wolves;
            [ReadOnly] public ComponentLookup<Sheep> SheepLookup;

            public ComponentLookup<Health> HealthLookup;
            public float DeltaTime;

            public void Execute(TriggerEvent triggerEvent)
            {
                ApplyDamage(triggerEvent.EntityA, triggerEvent.EntityB);
                ApplyDamage(triggerEvent.EntityB, triggerEvent.EntityA);
            }

            private void ApplyDamage(Entity wolfEntity, Entity sheepEntity)
            {
                if (!Wolves.HasComponent(wolfEntity) || !SheepLookup.HasComponent(sheepEntity)) 
                    return;
                if (!HealthLookup.HasComponent(sheepEntity)) return;

                Health health = HealthLookup[sheepEntity];
                health.Value -= Wolves[wolfEntity].DamagePerSecond * DeltaTime;
                HealthLookup[sheepEntity] = health;
            }
        }
    }
}