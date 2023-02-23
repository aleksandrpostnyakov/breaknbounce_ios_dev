using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemyShield : BrickBase
    {
        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _colliderObject.SetActive(true);
        }

        public class Pool : MonoMemoryPool<EnemyShield>
        {
            protected override void OnSpawned(EnemyShield item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
                item.DisableCollider();
            }
        }
    }
}