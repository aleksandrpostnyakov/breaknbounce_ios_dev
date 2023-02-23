using Zenject;

namespace Items
{
    public class Obstacle : BrickBase
    {
        public override bool UpdateHealth(int value, out int takenDamage, bool needEffect = false)
        {
            takenDamage = 0;
            return true;
        }

        public class Pool : MonoMemoryPool<Obstacle> { }
    }
}