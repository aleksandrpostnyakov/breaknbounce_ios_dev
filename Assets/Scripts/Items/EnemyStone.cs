using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemyStone : BrickBase
    {
        [SerializeField] private GameObject _gameObject;
        [SerializeField] private Renderer _unstonedRender;
        [SerializeField] private Renderer _stonedRender;
        private bool _stoned;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _colliderObject.SetActive(true);
            _stoned = false;
            _stonedRender.enabled = _stoned;
            _unstonedRender.enabled = !_stoned;
        }

        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            return new BrickPassiveMoveResult()
            {
                type = BrickPassiveMoveResultType.Stone,
                BrickId = Id,
                OnLastLine = onLastLine
            };
        }

        public void ChangeStone(bool directNotStone)
        {
            if (directNotStone)
            {
                _stoned = false;
            }
            else
            {
                _stoned = !_stoned;
            }
            _gameObject.tag = _stoned ? "BrickShield" : "Brick";
            _healthText.enabled = !_stoned;
            _stonedRender.enabled = _stoned;
            _unstonedRender.enabled = !_stoned;
        }

        public class Pool : MonoMemoryPool<EnemyStone>
        {
            protected override void OnSpawned(EnemyStone item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
                item.DisableCollider();
            }
        }
    }
}