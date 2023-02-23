using System;
using Config;
using UnityEngine;
using Zenject;

namespace Effects
{
    public class BossExplosiveEffect : CustomEffect
    {
        [SerializeField] private GameObject _explParticle;
        [SerializeField] private ParticleSystem _explParticleSystem;
        [SerializeField] private ParticleSystem _sparksAreaParticleSystem;
        [SerializeField] private ParticleSystem _smokeTrailParticleSystem;
        [SerializeField] private ParticleSystem _smokePuffParticleSystem;

        private Action _onComplete;

        private void Awake()
        {
            Clear();
        }

        private void Clear()
        {
            if (_explParticle != null)
            {
                _explParticle.SetActive(false);
            }
   
            _onComplete = null;
        }

        public override async void Init(Transform parent, Vector3 startPosition, float delay, BaseBrickConfig config, Action onComplete, bool isBoss = false)
        {
            Transform transform1;
            (transform1 = transform).SetParent(parent);
            transform1.position = startPosition;

            _onComplete = onComplete;
            
            var mainModule = _sparksAreaParticleSystem.main;
            var grad1 = new ParticleSystem.MinMaxGradient(config.BaseColor1, config.BaseColor2)
            {
                mode = ParticleSystemGradientMode.TwoColors
            };
            mainModule.startColor = grad1;
            
            var smoke = _smokeTrailParticleSystem.colorOverLifetime;
            var grad2 = new ParticleSystem.MinMaxGradient(config.BaseColor1, config.BaseColor1);
            smoke.color = grad2;
            
            var puff = _smokePuffParticleSystem.colorOverLifetime;
            var grad3 = new ParticleSystem.MinMaxGradient(config.BaseColor2, config.BaseColor2);
            puff.color = grad3;
            
            _explParticle.SetActive(true);
            _explParticleSystem.Play(true);

            await new WaitForSeconds(delay);
            _onComplete?.Invoke();
        }

        public class Pool : MonoMemoryPool<BossExplosiveEffect>
        {
            protected override void OnDespawned(BossExplosiveEffect item)
            {
                if (item == null)
                {
                    return;
                }
                base.OnDespawned(item);
                Clear();
            }
        }
    }
}