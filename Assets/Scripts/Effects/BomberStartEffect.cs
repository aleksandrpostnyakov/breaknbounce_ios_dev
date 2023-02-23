using System;
using Config;
using UnityEngine;
using Zenject;

namespace Effects
{
    public class BomberStartEffect : CustomEffect
    {
        [SerializeField] private GameObject _explParticle;
        [SerializeField] private ParticleSystem _explParticleSystem;

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
            transform1.localPosition = startPosition;

            _onComplete = onComplete;
            
            _explParticle.SetActive(true);
            _explParticleSystem.Play(true);
            
            _explParticle.SetActive(true);
            _explParticleSystem.Play(true);

            var duration = config switch
            {
                BossBomberConfig bossBomberConfig => bossBomberConfig.ExplodeTime,
                BomberConfig bomberConfig => bomberConfig.ExplodeTime,
                _ => delay
            };
            await new WaitForSeconds(duration);
            _onComplete?.Invoke();
        }
        
        public class Pool : MonoMemoryPool<BomberStartEffect>
        {
            protected override void OnDespawned(BomberStartEffect item)
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