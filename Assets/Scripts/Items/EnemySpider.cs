using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemySpider : BrickBase
    {
        private int _counter;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _colliderObject.SetActive(true);
            _counter = 0;
        }

        public override BrickActiveMoveResult ActiveAction(bool brickOnLastLine)
        {
            var spiderConfig = (SpiderConfig) _config;
            _counter++;
            
            return new BrickActiveMoveResult()
            {
                type = BrickActiveMoveResultType.Spider,
                BrickId = Id,
                ResultBool = _counter >= spiderConfig.CountOfJump,
                Attack = brickOnLastLine
            };
        }
        
        public override bool CanMove()
        {
            return false;
        }
        
        public class Pool : MonoMemoryPool<EnemySpider>
        {
            protected override void OnSpawned(EnemySpider item)
            {
                base.OnSpawned(item);
                item.transform.position = new Vector3(1000, 1000, 1000);
                item._healthText.enabled = false;
                item.DisableCollider();
            }
        }
    }
}