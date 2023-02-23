using System;
using Config;
using UnityEngine;
using Zenject;

namespace Effects
{
    public class PlayerDeadEffect : CustomEffect
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

            await new WaitForSeconds(delay);
            _onComplete?.Invoke();
        }
        
        public class Pool : MonoMemoryPool<PlayerDeadEffect>
        {
            protected override void OnDespawned(PlayerDeadEffect item)
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