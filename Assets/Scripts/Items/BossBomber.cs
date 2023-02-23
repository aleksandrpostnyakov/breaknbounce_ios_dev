using System;
using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class BossBomber : BrickShooter
    {
        private bool _attackStep;
        private bool _exploded;
        public event Action<BrickBase> OnExplode;
        public event Action<BrickBase> OnStartExplodeTimer;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _attackStep = false;
            _exploded = false;
        }
        
        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            var configStep = (BossBomberConfig) _config;
            if (!_attackStep)
            {
                return new BrickPassiveMoveResult()
                {
                    type = BrickPassiveMoveResultType.Clone,
                    BrickId = Id,
                    ResultInt = 10,
                    Count = configStep.LifeGeneration,
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
        
        public override bool UpdateHealth(int value, out int takenDamage, bool needEffect = false)
        {
            var oldHealth = Health;
            takenDamage = 0;
            if (Health <= 0)
            {
                return !_exploded;
            }
            
            Health += value;
            Health = Mathf.Clamp(Health, 0, Health);
            
            if (value < 0)
            {
                HitTween();
                takenDamage = oldHealth - Health;
            }
            
            
            if (Health == 0)
            {
                DisableCollider();
                Explode();
            }
            
            ShowHealth();
            return !_exploded;
        }
        
        private async void Explode()
        {
            OnStartExplodeTimer?.Invoke(this);
            var config = _config as BossBomberConfig;
            await new WaitForSeconds(config.ExplodeTime);
            _exploded = true;
            OnExplode?.Invoke(this);
        }

        public override bool CanMove()
        {
            return false;
        }
        
        public class Pool : MonoMemoryPool<BossBomber>
        {
        }
    }
}