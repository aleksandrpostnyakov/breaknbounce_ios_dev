using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemyHealer : BrickBase
    {
        private bool _hiled;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _colliderObject.SetActive(true);
            _hiled = false;
        }

        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            return new BrickPassiveMoveResult()
            {
                type = BrickPassiveMoveResultType.Heal,
                BrickId = Id
            };
        }

        public bool CanHeal()
        {
            _hiled = !_hiled;
            return _hiled;
        }

        public class Pool : MonoMemoryPool<EnemyHealer>
        {
            protected override void OnSpawned(EnemyHealer item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
                item.DisableCollider();
            }
        }
    }
}