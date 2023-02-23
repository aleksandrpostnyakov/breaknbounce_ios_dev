﻿using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class BossSpider : BrickShooter
    {
        private bool _attackStep;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _attackStep = false;
        }
        
        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            var configStep = (BossSpiderConfig) _config;
            if (!_attackStep)
            {
                return new BrickPassiveMoveResult()
                {
                    type = BrickPassiveMoveResultType.Clone,
                    BrickId = Id,
                    ResultInt = 8,
                    Count = configStep.LifeGeneration,
                    CanMove = false,
                    FromBoss = true
                };
            }
            else
            {
                return new BrickPassiveMoveResult()
                {
                    type = BrickPassiveMoveResultType.Shoot,
                    BrickId = Id,
                };
            }    
        }

        public override BrickActiveMoveResult ActiveAction(bool brickOnLastLine)
        {
           _attackStep = !_attackStep;
            
            return new BrickActiveMoveResult()
            {
                type = BrickActiveMoveResultType.JumpBoss,
                BrickId = Id,
            };
        }
        
        public override bool CanMove()
        {
            return false;
        }
        
        public class Pool : MonoMemoryPool<BossSpider>
        {
        }
    }
}