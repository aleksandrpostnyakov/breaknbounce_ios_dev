using UnityEngine;
using Zenject;

namespace Items
{
    public class PrismBrickNW : BrickBase
    {
        public class Pool : MonoMemoryPool<PrismBrickNW>
        {
            protected override void OnSpawned(PrismBrickNW item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
            }
        }
    }
}