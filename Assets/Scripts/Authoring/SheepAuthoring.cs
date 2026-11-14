using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Authoring
{
    public class SheepAuthoring : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private int health = 100;
        [SerializeField] private Vector3 targetPosition;

        [SerializeField, Min(0.1f)] private float detectionRadius = 8f;
        [SerializeField, Min(0.1f)] private float escapeDistance = 12f;
        [SerializeField, Min(0.1f)] private float avoidanceRadius = 3f;

        private class SheepAuthoringBaker : Baker<SheepAuthoring>
        {
            public override void Bake(SheepAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<Sheep>(entity);

                AddComponent(entity, new MoveSpeed
                {
                    Value = authoring.moveSpeed
                });

                AddComponent(entity, new MoveTarget
                {
                    Position = authoring.targetPosition
                });

                AddComponent(entity, new Health
                {
                    Value = authoring.health
                });

                AddComponent(entity, new Danger
                {
                    DetectionRadius = authoring.detectionRadius,
                    EscapeDistance = authoring.escapeDistance,
                    AvoidanceRadius = authoring.avoidanceRadius,
                    IsFleeing = false,
                    SteeringTarget = authoring.targetPosition,
                    AvoidedWolf = Entity.Null,
                    AvoidanceSide = 0f
                });
            }
        }
    }
}