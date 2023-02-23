using UnityEngine;
using Zenject;

namespace Level
{
    public class WarningIcon : MonoBehaviour
    {
        [SerializeField] public int Id;
        public void Init(Transform parent, float positionX, int id)
        {
            Transform transform1;
            (transform1 = transform).SetParent(parent);
            transform1.localPosition = new Vector3(positionX, 0, 0);
            Id = id;
        }
        
        public class Pool : MonoMemoryPool<WarningIcon> { }
    }
}