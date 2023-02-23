using System;
using Config;
using UnityEngine;

namespace Effects
{
    public class CustomEffect : MonoBehaviour
    {
        public virtual void Init(Transform parent, Vector3 startPosition, float delay, BaseBrickConfig config, Action onComplete, bool isBoss = false)
        {
            
        }
    }
}