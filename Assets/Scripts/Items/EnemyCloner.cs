using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemyCloner : BrickBase
    {
        private int _cloneStep;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _cloneStep = 0;
        }

        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            var configStep = (ClonerConfig) _config;
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
                ResultInt = 0
            };
        }
        public class Pool : MonoMemoryPool<EnemyCloner> { }
    }
}