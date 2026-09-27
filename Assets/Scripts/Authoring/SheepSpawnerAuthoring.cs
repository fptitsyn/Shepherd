using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public class SheepSpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject sheepPrefab;
        [SerializeField] private int count = 1000;
        [SerializeField] private int rowSize = 40;
        [SerializeField] private float spacing = 1.5f;
        
        private class SheepSpawnerAuthoringBaker : Baker<SheepSpawnerAuthoring>
        {
            public override void Bake(SheepSpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                Entity prefabEntity = GetEntity(authoring.sheepPrefab, TransformUsageFlags.Dynamic);
                
                AddComponent(entity, new SheepSpawner
                {
                    Prefab = prefabEntity,
                    Count = authoring.count,
                    RowSize = authoring.rowSize,
                    Spacing = authoring.spacing
                });
            }
        }
    }
}