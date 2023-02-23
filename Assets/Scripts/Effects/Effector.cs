using System;
using Config;
using Interhaptics.Platforms.Mobile;
using Sound;
using UnityEngine;
using Zenject;

namespace Effects
{
    public class Effector
    {
        [Inject] private BulletEffect.Pool _bulletEffectPool;
        [Inject] private ExplosiveEffect.Pool _explosiveEffectPool;
        [Inject] private BossExplosiveEffect.Pool _bossExplosiveEffectPool;
        [Inject] private BomberStartEffect.Pool _bomberStartEffectPool;
        [Inject] private BombExplosiveEffect.Pool _bomberExplosiveEffectPool;
        [Inject] private HealEffect.Pool _healEffectPool;
        [Inject] private HitStoneEffect.Pool _hitStoneEffectPool;
        [Inject] private PlayerDeadEffect.Pool _playerdeadEffectPool;
        
        [Inject] private MobileHapticsVibration _vibration;
        [Inject] private ISoundManager _soundManager;

        public void AddFlyEffect(FlyEffectType type, Transform parent, Vector3 startPosition, Vector3 finishPosition, float duration, BaseBrickConfig config, Action onHit, Action onComplete)
        {
            var effect = CreateFlyEffect(type);
            effect.Init(parent, startPosition, finishPosition, duration, config, onHit, () =>
            {
                DestroyFlyEffect(effect);
                onComplete?.Invoke();
            });
        }
        
        public void AddCustomEffect(CustomEffectType type, Transform parent, Vector3 startPosition, float duration, BaseBrickConfig config, Action onComplete, bool isBoss = false)
        {
            var effect = CreateCustomEffect(type);
            effect.Init(parent, startPosition, duration, config, () =>
            {
                DestroyCustomEffect(effect);
                onComplete?.Invoke();
            }, isBoss);

            if (_soundManager.Settings.VibroOn)
            {
                switch (type)
                {
                    case CustomEffectType.EnemyExplosive:
                        _vibration.PlayVibration(3);
                        break;
                    case CustomEffectType.BossExplosive:
                        _vibration.PlayVibration(4);
                        break;
                    case CustomEffectType.PlayerDead:
                        _vibration.PlayVibration(5);
                        break;
                }
            }
        }

        private FlyEffect CreateFlyEffect(FlyEffectType effectType)
        {
            return effectType switch
            {
                FlyEffectType.Bullet => _bulletEffectPool.Spawn(),
                FlyEffectType.Heal => _healEffectPool.Spawn(),
                _ => throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null)
            };
        }

        private void DestroyFlyEffect(FlyEffect effect)
        {
            switch (effect)
            {
                case BulletEffect bulletEffect:
                    _bulletEffectPool.Despawn(bulletEffect);
                    break;
                case HealEffect healEffect:
                    _healEffectPool.Despawn(healEffect);
                    break;
            }
        }
        
        private CustomEffect CreateCustomEffect(CustomEffectType effectType)
        {
            return effectType switch
            {
                CustomEffectType.EnemyExplosive => _explosiveEffectPool.Spawn(),
                CustomEffectType.BossExplosive => _bossExplosiveEffectPool.Spawn(),
                CustomEffectType.BomberStart => _bomberStartEffectPool.Spawn(),
                CustomEffectType.BombExplosive => _bomberExplosiveEffectPool.Spawn(),
                CustomEffectType.StoneHit => _hitStoneEffectPool.Spawn(),
                CustomEffectType.PlayerDead => _playerdeadEffectPool.Spawn(),
                _ => throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null)
            };
        }

        private void DestroyCustomEffect(CustomEffect effect)
        {
            switch (effect)
            {
                case ExplosiveEffect bulletEffect:
                    _explosiveEffectPool.Despawn(bulletEffect);
                    break;
                case BossExplosiveEffect explosiveEffect:
                    _bossExplosiveEffectPool.Despawn(explosiveEffect);
                    break;
                case BomberStartEffect bomberStartEffect:
                    _bomberStartEffectPool.Despawn(bomberStartEffect);
                    break;
                case BombExplosiveEffect bomberExplosiveEffect:
                    _bomberExplosiveEffectPool.Despawn(bomberExplosiveEffect);
                    break;
                case HitStoneEffect hitStoneEffect:
                    _hitStoneEffectPool.Despawn(hitStoneEffect);
                    break;
                case PlayerDeadEffect playerDeadEffect:
                    _playerdeadEffectPool.Despawn(playerDeadEffect);
                    break;
            }
        }
    }

    public enum FlyEffectType
    {
        Bullet,
        Heal
    }

    public enum CustomEffectType
    {
        EnemyExplosive,
        BossExplosive,
        BomberStart,
        BombExplosive,
        StoneHit,
        PlayerDead
    }
}