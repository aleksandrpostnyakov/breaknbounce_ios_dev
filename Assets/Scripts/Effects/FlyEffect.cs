using System;
using Config;
using UnityEngine;

namespace Effects
{
    public class FlyEffect : MonoBehaviour
    {
        public virtual void Init(Transform parent, Vector3 startPosition, Vector3 finishPosition, float delay, BaseBrickConfig config, Action onHit, Action onComplete)
        {
            
        }
    }
}