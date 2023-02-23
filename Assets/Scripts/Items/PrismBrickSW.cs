using UnityEngine;
using Zenject;

namespace Items
{
    public class PrismBrickSW : BrickBase
    {
        public class Pool : MonoMemoryPool<PrismBrickSW>
        {
            protected override void OnSpawned(PrismBrickSW item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
            }
        }
    }
}