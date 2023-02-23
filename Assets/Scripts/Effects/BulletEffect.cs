using System;
using Config;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Effects
{
    public class BulletEffect : FlyEffect
    {
        [SerializeField] private GameObject _startParticle;
        [SerializeField] private GameObject _flyParticle;
        [SerializeField] private GameObject _endParticle;
        [SerializeField] private ParticleSystem _startParticleSystem;
        [SerializeField] private ParticleSystem _flyParticleSystem;
        [SerializeField] private ParticleSystem _endParticleSystem;
        [SerializeField] private ParticleSystem _glowParticleSystem;

        private Action _onHit;
        private Action _onComplete;

        private void Awake()
        {
            Clear();
        }

        private void Clear()
        {
            if (_startParticle != null)
            {
                _startParticle.SetActive(false);
            }
            if (_flyParticle != null)
            {
                _flyParticle.SetActive(false);
            }
            if (_endParticle != null)
            {
                _endParticle.SetActive(false);
            }

            _onHit = null;
            _onComplete = null;
        }

        public override void Init(Transform parent, Vector3 startPosition, Vector3 finishPosition, float delay, BaseBrickConfig config, Action onHit, Action onComplete)
        {
            Transform transform1;
            (transform1 = transform).SetParent(parent);
            transform1.position = startPosition;
            transform.LookAt(finishPosition);

            _onHit = onHit;
            _onComplete = onComplete;
            
            var flyParticle = _flyParticleSystem.main;
            flyParticle.startColor = new ParticleSystem.MinMaxGradient(config.BaseColor2);

            var glow = _glowParticleSystem.main;
            glow.startColor = new ParticleSystem.MinMaxGradient(config.BaseColor1);
            
            _startParticle.SetActive(true);
            _startParticleSystem.Play(true);
            
            Move(finishPosition, delay);
        }

        private void Move(Vector3 finishPosition, float delay)
        {
            _flyParticle.SetActive(true);
            _flyParticleSystem.Play(true);
            transform.DOMove(finishPosition, delay).OnComplete(async () =>
            {
                _flyParticle.SetActive(false);
                _flyParticleSystem.Stop(true);
                
                _endParticle.SetActive(true);
                _endParticleSystem.Play(true);
                
                _onHit?.Invoke();
                await new WaitForSeconds(1f);
                _onComplete?.Invoke();
            });
        }

        public class Pool : MonoMemoryPool<BulletEffect>
        {
            protected override void OnDespawned(BulletEffect item)
            {
                base.OnDespawned(item);
                item.Clear();
            }
        }
    }
}