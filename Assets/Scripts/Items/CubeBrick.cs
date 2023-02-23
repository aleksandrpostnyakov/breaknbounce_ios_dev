using UnityEngine;
using Zenject;

namespace Items
{
    public class CubeBrick : BrickBase
    {
        public class Pool : MonoMemoryPool<CubeBrick>
        {
            protected override void OnSpawned(CubeBrick item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
            }
        }
    }
}