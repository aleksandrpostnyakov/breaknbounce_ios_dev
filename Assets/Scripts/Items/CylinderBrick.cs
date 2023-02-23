using UnityEngine;
using Zenject;

namespace Items
{
    public class CylinderBrick: BrickBase
    {

        public class Pool : MonoMemoryPool<CylinderBrick>
        {
            protected override void OnSpawned(CylinderBrick item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
            }
        }
    }
}