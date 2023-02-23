using System;
using Config;
using UnityEngine;
using Zenject;

namespace Effects
{
    public class BombExplosiveEffect : CustomEffect
    {
        [SerializeField] private GameObject _explParticle;
        [SerializeField] private ParticleSystem _explParticleSystem;

        private Action _onComplete;
        private Vector3 _localScale;

        private void Awake()
        {
            Clear();
            _localScale = transform.localScale;
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
            transform1.localScale = _localScale * (isBoss ? 2 : 1);

            _onComplete = onComplete;
            
            _explParticle.SetActive(true);
            _explParticleSystem.Play(true);

            await new WaitForSeconds(delay);
            _onComplete?.Invoke();
        }
        
        public class Pool : MonoMemoryPool<BombExplosiveEffect>
        {
            protected override void OnDespawned(BombExplosiveEffect item)
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