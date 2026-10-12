using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class PrefabHolderAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject sheepPrefab;
        
        private class PrefabHolderAuthoringBaker : Baker<PrefabHolderAuthoring>
        {
            public override void Bake(PrefabHolderAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new PrefabHolder
                {
                    SheepPrefab = GetEntity(authoring.sheepPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}