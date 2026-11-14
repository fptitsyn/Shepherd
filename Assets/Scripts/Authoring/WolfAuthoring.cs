using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class WolfAuthoring : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float damagePerSecond = 25f;

        private class WolfAuthoringBaker : Baker<WolfAuthoring>
        {
            public override void Bake(WolfAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new Wolf
                {
                    DamagePerSecond = authoring.damagePerSecond
                });
            }
        }
    }
}