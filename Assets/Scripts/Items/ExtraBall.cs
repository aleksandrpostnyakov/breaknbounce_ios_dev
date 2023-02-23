using Zenject;

namespace Items
{
    public class ExtraBall : BrickBase
    {
        public override bool UpdateHealth(int value, out int takenDamage, bool needEffect = false)
        {
            takenDamage = 0;
            return value == 0;
        }
        
        public class Pool : MonoMemoryPool<ExtraBall> { }
    }
}