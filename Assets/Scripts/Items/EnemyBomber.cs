using System;
using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemyBomber : BrickBase
    {
        private bool _exploded;
        public event Action<EnemyBomber> OnExplode;
        public event Action<BrickBase> OnStartExplodeTimer;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _colliderObject.SetActive(true);
            _exploded = false;
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
            Health = Mathf.Clamp(Health, 0, BaseHealth);
            
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
            var config = _config as BomberConfig;
            await new WaitForSeconds(config.ExplodeTime);
            _exploded = true;
            OnExplode?.Invoke(this);
        }

        public class Pool : MonoMemoryPool<EnemyBomber>
        {
            protected override void OnSpawned(EnemyBomber item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
                item.DisableCollider();
            }
        }
    }
}