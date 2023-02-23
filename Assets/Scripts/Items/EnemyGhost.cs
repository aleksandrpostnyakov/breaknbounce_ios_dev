using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class EnemyGhost : BrickBase
    {
        [SerializeField] private Renderer _renderer;
        private bool _hided;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _colliderObject.SetActive(true);
            _hided = false;
        }

        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            return new BrickPassiveMoveResult()
            {
                type = BrickPassiveMoveResultType.Ghost,
                BrickId = Id
            };
        }

        public void ChangeGhost(bool directNotHided)
        {
            _hided = !directNotHided && !_hided;

            var material = _renderer.material;
            var color = material.color;
            material.color = new Color() {a = _hided ? .5f : 1, r = color.r, g = color.g, b = color.b};
            var textColor = _healthText.color;
            _healthText.color = new Color() {a = _hided ? 0f : 1, r = textColor.r, g = textColor.g, b = textColor.b};
            _colliderObject.SetActive(!_hided);
        }

        public class Pool : MonoMemoryPool<EnemyGhost>
        {
            protected override void OnSpawned(EnemyGhost item)
            {
                base.OnSpawned(item);
                item._healthText.enabled = false;
                item.DisableCollider();
            }
        }
    }
}