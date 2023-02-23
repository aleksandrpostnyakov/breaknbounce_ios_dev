using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class BossStone : BrickBase
    {
        [SerializeField] private GameObject _gameObject;
        [SerializeField] private Renderer _unstonedRender;
        [SerializeField] private Renderer _stonedRender;
        private bool _stoned;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _stoned = false;
            _stonedRender.enabled = _stoned;
            _unstonedRender.enabled = !_stoned;
        }

        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            var configStep = (BossStoneConfig) _config;
            return new BrickPassiveMoveResult()
            {
                type = BrickPassiveMoveResultType.StoneBoss,
                BrickId = Id,
                ResultInt = 6,
                Count = !_stoned ? configStep.LifeGeneration : 0,
                FromBoss = true
            };
        }

        public void ChangeStone(bool directNotStone)
        {
            _stoned = !_stoned;
            _gameObject.tag = directNotStone ? "Brick" : (_stoned ? "BrickShield" : "Brick");
            _healthText.enabled = directNotStone || !_stoned;
            _stonedRender.enabled = !directNotStone && _stoned;
            _unstonedRender.enabled = directNotStone || !_stoned;
        }

        public class Pool : MonoMemoryPool<BossStone> { }
    }
}