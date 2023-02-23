using System;
using Config;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Zenject;
using Random = UnityEngine.Random;

namespace Items
{
    public class BrickBase : MonoBehaviour, IBrick
    {
        [Inject] private GameConfig _gameConfig;
        
        [SerializeField] protected TMP_Text _healthText;
        [SerializeField] protected GameObject _colliderObject;
        [SerializeField] protected GameObject _meshObject;
        [SerializeField] protected Transform _effectPoint;

        private Tweener _tween;
        private Sequence _hitSequence;
        private Sequence _jumpSequence;
        protected BaseBrickConfig _config;
        protected Vector3 _originalScale;
        
        public int Id { get; private set; }
        public BrickType TypeOfBrick { get; protected set; }
        public int Health { get; protected set; }
        public int BaseHealth { get; protected set; }

        private void Start()
        {
            if (_meshObject != null)
            {
                _originalScale = _meshObject.transform.localScale;
            }
        }

        public virtual void Init(BrickType type, Transform tr, Vector3 position, int id, int baseHealth, BaseBrickConfig config = null)
        {
            if (_colliderObject == null)
            {
                _colliderObject = gameObject;
            }
            
            _colliderObject.SetActive(true);
            
            _config = config;
            Transform transform1;
            (transform1 = transform).SetParent(tr);
            transform1.position = position;

            TypeOfBrick = type;
            Id = id;
            float multiplier = 1;
            
            if (BrickTypeHelper.IsEnemy(TypeOfBrick))
            {
                var enemyConfig = (BaseEnemyConfig) _config;
                multiplier = enemyConfig.StartHpCoefficient;
            }
            else if (BrickTypeHelper.IsBoss(TypeOfBrick))
            {
                var enemyConfig = (BaseEnemyConfig) _config;
                multiplier *= enemyConfig.StartHpCoefficient;
            }
            
            Health = BaseHealth = Mathf.CeilToInt(baseHealth * multiplier);
            UpdateHealth(0, out var takenDamage);
            if (_healthText != null)
            {
                _healthText.enabled = true;
            }
        }

        public Vector3 GetPosition(float baseSize)
        {
            if (BrickTypeHelper.IsBoss(TypeOfBrick))
            {
                return transform.position + new Vector3(baseSize * .5f, 0, -baseSize * .5f);
            }

            return transform.position;
        }

        public Transform GetEffectPointTransform()
        {
            return _effectPoint;
        }

        public GameObject GetMainObject()
        {
            return _colliderObject;
        }

        public int GetPriority()
        {
            return _config?.Priority ?? 0;
        }

        public float GetHealthCoeff()
        {
            return (float)Health / BaseHealth;
        }

        public int GetAttackValue()
        {
            if (BrickTypeHelper.IsCommon(TypeOfBrick))
            {
                return Mathf.CeilToInt(Health);
            }
            
            var enemyConfig = (BaseEnemyConfig) _config;
            if (enemyConfig == null)
            {
                return 0;
            }
            var multiplier = enemyConfig.DamageCoefficient;
            
            if (BrickTypeHelper.IsEnemy(TypeOfBrick))
            {
                return Mathf.CeilToInt(Health * multiplier);
            }
            if (BrickTypeHelper.IsBoss(TypeOfBrick))
            {
                return Mathf.CeilToInt(BaseHealth / enemyConfig.StartHpCoefficient * multiplier);
            }

            return 0;
        }
        

        public virtual bool UpdateHealth(int value, out int takenDamage, bool needEffect = false)
        {
            var oldHealth = Health;
            Health += value;
            Health = Mathf.Clamp(Health, 0, BaseHealth);
            ShowHealth();
            takenDamage = 0;
            if (value < 0)
            {
                HitTween();
                takenDamage = oldHealth - Health;
            }

            return Health > 0;
        }

        protected void ShowHealth()
        {
            var healthString = Health.ToString();
            if (Health > 1000)
            {
                healthString = ((int) Health / 1000).ToString() + "K";
            }
            _healthText.text = healthString;
        }
        
        public void DisableCollider()
        {
            _colliderObject.SetActive(false);
        }

        public virtual BrickPassiveMoveResult PassiveAction(bool onLastLine = false)
        {
            return null;
        }
        
        public virtual BrickActiveMoveResult ActiveAction(bool brickOnLastLine)
        {
            return null;
        }

        public virtual bool CanMove()
        {
            return true;
        }

        public virtual void EndTurn()
        {
            
        }

        public virtual void Move(Vector2 pos)
        {
            transform.position += new Vector3(pos.y, 0, -pos.x);
        }

        public virtual void TweenMove(Vector3 pos, float duration, Action callback)
        {
            _tween.Kill();
            _tween = transform.DOMove(pos, duration).OnComplete(() =>
            {
                callback?.Invoke();
            });
        }

        public virtual void HitTween()
        {
            if (_meshObject == null)
            {
                return;
            }
            _hitSequence.Kill();
            var scaleSpeed = _gameConfig.GetDefaultFieldConfig.HitScaleTime;
            var scaleCoef = _gameConfig.GetDefaultFieldConfig.HitScaleCoefficient;
            _hitSequence = DOTween.Sequence()
                .Append(_meshObject.transform.DOScale(new Vector3(_originalScale.x * scaleCoef, _originalScale.y, _originalScale.z * scaleCoef), scaleSpeed))
                .Append(_meshObject.transform.DOScale(_originalScale, scaleSpeed));

            _hitSequence.Play();
        }
        
        public virtual void JumpTween()
        {
            if (_meshObject == null || _jumpSequence != null)
            {
                return;
            }

            var defValue = _meshObject.transform.position.y;
            _jumpSequence = DOTween.Sequence()
                .Append(_meshObject.transform.DOMoveY(defValue + .4f, .2f))
                .Append(_meshObject.transform.DOMoveY(defValue, .2f))
                .OnComplete(() =>
                {
                    _jumpSequence = null;
                });

            _jumpSequence.Play();
        }
    }

    public class BrickPassiveMoveResult
    {
        public BrickPassiveMoveResultType type;
        public int BrickId;
        public int ResultInt;
        public int Count = 1;
        public bool CanMove = true;
        public bool FromBoss;
        public bool OnLastLine;
    }
    
    public class BrickActiveMoveResult
    {
        public BrickActiveMoveResultType type;
        public int BrickId;
        public bool ResultBool;
        public bool Attack;
        public bool IsBoss;
    }

    public enum BrickPassiveMoveResultType
    { 
        Ghost,
        Stone,
        Clone,
        Heal,
        GhostBoss,
        StoneBoss,
        Shoot
    }
    
    public enum BrickActiveMoveResultType
    { 
        Spider,
        Ram,
        JumpBoss
    }
    
}