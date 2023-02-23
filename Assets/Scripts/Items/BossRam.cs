using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class BossRam : BrickBase
    {
        private int _cloneStep;
        
        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _cloneStep = 1;
        }
        
        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            var configStep = (BossRamConfig) _config;
            _cloneStep++;
            if (_cloneStep != configStep.StepOfClone)
            {
                return null;
            }
            _cloneStep = 0;
                
            return new BrickPassiveMoveResult()
            {
                type = BrickPassiveMoveResultType.Clone,
                BrickId = Id,
                ResultInt = 4,
                Count = configStep.LifeGeneration,
                CanMove = false,
                FromBoss = true
            };
        }
        
        public override BrickActiveMoveResult ActiveAction(bool brickOnLastLine)
        {
            return new BrickActiveMoveResult()
            {
                type = BrickActiveMoveResultType.Ram,
                BrickId = Id,
                Attack = brickOnLastLine,
                IsBoss = true
            };
        }

        public override bool CanMove()
        {
            return false;
        }

        public class Pool : MonoMemoryPool<BossRam> { }
    }
}