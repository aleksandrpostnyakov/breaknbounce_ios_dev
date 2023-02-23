using System;
using Config;
using UnityEngine;
using Zenject;

namespace Effects
{
    public class ExplosiveEffect : CustomEffect
    {
        [SerializeField] private GameObject _explParticle;
        [SerializeField] private ParticleSystem _explParticleSystem;
        [SerializeField] private ParticleSystem _areaParticleSystem;
        [SerializeField] private ParticleSystem _outerAreaParticleSystem;
        [SerializeField] private ParticleSystem _sparksAreaParticleSystem;

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
            
            var aa = _sparksAreaParticleSystem.main;
            var grad = new ParticleSystem.MinMaxGradient(config.BaseColor1, config.BaseColor2)
            {
                mode = ParticleSystemGradientMode.TwoColors
            };
            aa.startColor = grad;
            
            _explParticle.SetActive(true);
            _explParticleSystem.Play(true);

            await new WaitForSeconds(delay);
            _onComplete?.Invoke();
        }
        
        public class Pool : MonoMemoryPool<ExplosiveEffect>
        {
            protected override void OnDespawned(ExplosiveEffect item)
            {
                if (item != null)
                {
                    base.OnDespawned(item);
                    Clear();
                }
            }
        }
    }
}