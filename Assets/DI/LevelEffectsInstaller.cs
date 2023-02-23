using Effects;
using Items;
using UnityEngine;
using Zenject;

namespace DI
{
    public class LevelEffectsInstaller : MonoInstaller
    {
        [SerializeField] private BulletEffect _bulletEffectPrefab;
        [SerializeField] private ExplosiveEffect _explosiveEffectPrefab;
        [SerializeField] private BossExplosiveEffect _bossExplosiveEffectPrefab;
        [SerializeField] private BomberStartEffect _bomberStartEffectPrefab;
        [SerializeField] private BombExplosiveEffect _bomberExplosiveEffectPrefab;
        [SerializeField] private HealEffect _healEffectPrefab;
        [SerializeField] private HitStoneEffect _hitStoneEffectPrefab;
        [SerializeField] private PlayerDeadEffect _playerDeadEffectPrefab;
        public override void InstallBindings()
        {
            Container.BindMemoryPool<BulletEffect, BulletEffect.Pool>().FromComponentInNewPrefab(_bulletEffectPrefab);
            Container.BindMemoryPool<ExplosiveEffect, ExplosiveEffect.Pool>().FromComponentInNewPrefab(_explosiveEffectPrefab);
            Container.BindMemoryPool<BossExplosiveEffect, BossExplosiveEffect.Pool>().FromComponentInNewPrefab(_bossExplosiveEffectPrefab);
            Container.BindMemoryPool<BomberStartEffect, BomberStartEffect.Pool>().FromComponentInNewPrefab(_bomberStartEffectPrefab);
            Container.BindMemoryPool<BombExplosiveEffect, BombExplosiveEffect.Pool>().FromComponentInNewPrefab(_bomberExplosiveEffectPrefab);
            Container.BindMemoryPool<HealEffect, HealEffect.Pool>().FromComponentInNewPrefab(_healEffectPrefab);
            Container.BindMemoryPool<HitStoneEffect, HitStoneEffect.Pool>().FromComponentInNewPrefab(_hitStoneEffectPrefab);
            Container.BindMemoryPool<PlayerDeadEffect, PlayerDeadEffect.Pool>().FromComponentInNewPrefab(_playerDeadEffectPrefab);
        }
    }
}