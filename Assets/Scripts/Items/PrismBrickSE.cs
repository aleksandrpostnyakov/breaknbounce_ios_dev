using UnityEngine;
using Zenject;

namespace Items
{
    public class PrismBrickSE : BrickBase
    {
        public class Pool : MonoMemoryPool<PrismBrickSE>
        {
            protected override void OnSpawned(PrismBrickSE item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
            }
        }
    }
}