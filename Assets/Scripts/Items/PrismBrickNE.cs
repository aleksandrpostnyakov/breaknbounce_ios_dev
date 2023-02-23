using UnityEngine;
using Zenject;

namespace Items
{
    public class PrismBrickNE : BrickBase
    {
        public class Pool : MonoMemoryPool<PrismBrickNE>
        {
            protected override void OnSpawned(PrismBrickNE item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
            }
        }
    }
}