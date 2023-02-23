using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class BossShield : BrickBase
    {
        private int _cloneStep;
        
        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _cloneStep = 1;
        }
        
        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            var configStep = (BossShieldConfig) _config;
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
                ResultInt = 7,
                Count = configStep.LifeGeneration,
                FromBoss = true
            };
        }
        public class Pool : MonoMemoryPool<BossShield> { }
    }
}