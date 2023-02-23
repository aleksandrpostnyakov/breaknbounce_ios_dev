using Config;
using UnityEngine;
using Zenject;

namespace Items
{
    public class BossGhost : BrickBase
    {
        [SerializeField] private Renderer _renderer;
        private bool _hided;

        public override void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            base.Init(type, tr, position, id, baseHealth, config);
            _hided = false;
            SetMaterials(false);
        }

        public override BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            var configStep = (BossGhostConfig) _config;
            
            return new BrickPassiveMoveResult()
            {
                type = BrickPassiveMoveResultType.GhostBoss,
                BrickId = Id,
                ResultInt = 5,
                Count = !_hided ? configStep.LifeGeneration : 0,
                FromBoss = true
            };
        }

        public void ChangeGhost(bool directNotHided)
        {
            _hided = !_hided;

            SetMaterials(!directNotHided && _hided);
        }

        private void SetMaterials(bool hided)
        {
            var material = _renderer.material;
            var color = material.color;
            material.color = new Color() {a = hided ? .5f : 1, r = color.r, g = color.g, b = color.b};
            var textColor = _healthText.color;
            _healthText.color = new Color() {a = hided ? 0f : 1, r = textColor.r, g = textColor.g, b = textColor.b};
            _colliderObject.SetActive(!hided);
        }

        public class Pool : MonoMemoryPool<BossGhost> { }
    }
}