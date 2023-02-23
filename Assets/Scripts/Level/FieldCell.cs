using DG.Tweening;
using Items;
using UnityEngine;
using Zenject;

namespace Level
{
    public class FieldCell : MonoBehaviour
    {
        [SerializeField] private Renderer _renderer;

        public void Create(Transform parent, Vector3 position, Color color, float duration)
        {
            transform.SetParent(parent);
            transform.position = position + new Vector3(0, -10, 0); 
            _renderer.material.color = color;
            
            Move(position, duration);
        }

        private void Move(Vector3 position, float duration)
        {
            transform.DOMove(position, duration);
        }
        
        public class Pool : MonoMemoryPool<FieldCell> { }
    }
}