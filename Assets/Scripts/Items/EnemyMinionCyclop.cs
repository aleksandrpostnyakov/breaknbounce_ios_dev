using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemyMinionCyclop : BrickBase
    {
        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _colliderObject.SetActive(true);
        }

        public class Pool : MonoMemoryPool<EnemyMinionCyclop>
        {
            protected override void OnSpawned(EnemyMinionCyclop item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
                item.DisableCollider();
            }
        }
    }
}