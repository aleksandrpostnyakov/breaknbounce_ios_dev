using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemyRam : BrickBase
    {
        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _colliderObject.SetActive(true);
        }
        
        public override BrickActiveMoveResult ActiveAction(bool brickOnLastLine)
        {
            return new BrickActiveMoveResult()
            {
                type = BrickActiveMoveResultType.Ram,
                BrickId = Id,
                Attack = brickOnLastLine
            };
        }
        
        public override bool CanMove()
        {
            return false;
        }
        
        public class Pool : MonoMemoryPool<EnemyRam>
        {
            protected override void OnSpawned(EnemyRam item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
                item.DisableCollider();
            }
        }
    }
}