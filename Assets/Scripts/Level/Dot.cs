using Config;
using UnityEngine;
using Zenject;

namespace Level
{
    public class Dot: MonoBehaviour
    {
        [SerializeField] private RectTransform ImageTransform;
        public void Init(Transform parent, Vector3 position, float size)
        {
            Transform transform1;
            (transform1 = transform).SetParent(parent);
            transform1.position = position;
            ImageTransform.sizeDelta = new Vector2(size, size);
        }
        public class Pool : MonoMemoryPool<Dot> { }
    }
}