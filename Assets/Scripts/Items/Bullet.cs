using System;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace Items
{
    public class Bullet : MonoBehaviour
    {
        public void Init(Vector3 startPos, Vector3 finishPosition, float duration, Action onComplete)
        {
            transform.position = startPos;

            transform.DOMove(finishPosition, duration).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        }
        
        public class Pool : MonoMemoryPool<Bullet> { }
    }
}