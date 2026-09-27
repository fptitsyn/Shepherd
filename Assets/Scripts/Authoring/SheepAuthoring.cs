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
                    Position = new float3(
                        authoring.targetPosition.x,
                        authoring.targetPosition.y,
                        authoring.targetPosition.z
                    )
                });
                
                AddComponent(entity, new Health 
                {
                    Value = authoring.health
                });
            }
        }
    }
}