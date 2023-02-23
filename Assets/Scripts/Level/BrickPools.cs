using System;
using System.Threading.Tasks;
using Config;
using DG.Tweening;
using Items;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace Level
{
    public class BrickPools
    {
        [Inject] private CubeBrick.Pool _cubeBrickPool;
        [Inject] private CylinderBrick.Pool _cylinderBrickPool;
        [Inject] private PrismBrickNE.Pool _prismNEBrickPool;
        [Inject] private PrismBrickNW.Pool _prismNWBrickPool;
        [Inject] private PrismBrickSE.Pool _prismSEBrickPool;
        [Inject] private PrismBrickSW.Pool _prismSWBrickPool;
        [Inject] private EnemyMinion.Pool _enemyMinionPool;
        [Inject] private EnemyMinionCyclop.Pool _enemyMinionCyclopPool;
        [Inject] private EnemyGhost.Pool _enemyGhostPool;
        [Inject] private EnemyShield.Pool _enemyShieldPool;
        [Inject] private EnemyStone.Pool _enemyStonePool;
        [Inject] private EnemyCloner.Pool _enemyClonerPool;
        [Inject] private EnemyClonerSpecial.Pool _enemyClonerSpecialPool;
        [Inject] private EnemySpider.Pool _enemySpiderPool;
        [Inject] private EnemyRam.Pool _enemyRamPool;
        [Inject] private EnemyHealer.Pool _enemyHealerPool;
        [Inject] private EnemyBomber.Pool _enemyBomberPool;
        [Inject] private BossMinion.Pool _bossMinionPool;
        [Inject] private BossRam.Pool _bossRamPool;
        [Inject] private BossGhost.Pool _bossGhostPool;
        [Inject] private BossStone.Pool _bossStonePool;
        [Inject] private BossShield.Pool _bossShieldPool;
        [Inject] private BossSpider.Pool _bossSpiderPool;
        [Inject] private BossCloner.Pool _bossClonerPool;
        [Inject] private BossClonerSpecial.Pool _bossClonerSpecialPool;
        [Inject] private BossHealer.Pool _bossHealerPool;
        [Inject] private BossBomber.Pool _bossBomberPool;
        [Inject] private Obstacle.Pool _obstaclePool;
        [Inject] private ExtraBall.Pool _extraBallPool;
        
        [Inject] private Field _field;
        [Inject] private BricksMover _bricksMover;
        [Inject] private GameConfig _gameConfig;
        [Inject] private EnemiesConfig _enemiesConfig;
        
        public void CreateObstacle(Vector2Int position, int currentWave)
        {
            var obstacle = Create(BrickType.Obstacle);
            obstacle.Init(BrickType.Obstacle, _field.transform, new Vector3(_field.StartSpawnX + position.y * _field.GetBaseSize(), -_field.GetBaseSize() * .25f, _field.StartSpawnY - position.x * _field.GetBaseSize()), -1, currentWave);
            _bricksMover.AddBrick(position.x, position.y, new FieldBrickInfo(){Id = obstacle.Id, CanMove = obstacle.CanMove()});
        }
        
        public BrickBase CreateBrick(BrickType type, LevelConfig levelConfig, int currentWave)
        {
            var place = _bricksMover.GetFreeGenerationLineBrickPlace();
            if (place == -1)
            {
                return null;
            }

            var brick = Create(type);

            BaseBrickConfig brickConfig = null;
            if (BrickTypeHelper.IsEnemy(type))
            {
                brickConfig = _enemiesConfig.GetConfig(type);
            }
            
            brick.Init(type, _field.transform, new Vector3(_field.StartSpawnX + place * _field.GetBaseSize(), 0, _field.StartSpawnY - 1), 
                _bricksMover.GetCurrentBrickId(true), levelConfig.StartObstacleHP + (currentWave - 1) * levelConfig.IncrementObstacleHP, brickConfig);
            _bricksMover.AddBrick(1, place, new FieldBrickInfo(){Id = brick.Id, CanMove = brick.CanMove(), IsMinion = type == BrickType.EnemyMinion});
            return brick;
        }
        
        public BrickBase CreateBoss(BrickType type, LevelConfig levelConfig, int currentWave)
        {
            var brick = Create(type);
            var place = UnityEngine.Random.Range(0, _field.NumberOfCols - 2);
            var baseHealth = (int)((levelConfig.StartObstacleHP + (currentWave - 1) * levelConfig.IncrementObstacleHP) * levelConfig.TutorialBoss.TutorialBossHP);
            brick.Init(type, _field.transform, new Vector3(_field.StartSpawnX + place * _field.GetBaseSize(), 0, _field.StartSpawnY - 1), _bricksMover.GetCurrentBrickId(true), baseHealth, _enemiesConfig.GetConfig(type) );
            _bricksMover.AddBoss(1, place, new FieldBrickInfo(){Id = brick.Id, IsBoss = true, CanMove = brick.CanMove()});
            return brick;
        }
        
        public bool CreateGeneratedBrick(GenerateBrickData generateBrickData, Action<BrickBase> OnComplete)
        {
            var place = _bricksMover.GetNearestFreePlace(generateBrickData.ParentPosition);
            if (place.Equals(Vector2.negativeInfinity))
            {
                return false;
            }

            var brick = Create(generateBrickData.Type);

            BaseBrickConfig brickConfig = _enemiesConfig.GetConfig(generateBrickData.Type);
            
            var position = new  Vector2Int((int)place.x, (int)place.y);
            var startPosititon = generateBrickData.ParentPosition;
            var finishPosition = new Vector3(_field.StartSpawnX + place.y * _field.GetBaseSize(), 0,
                _field.StartSpawnY - place.x * _field.GetBaseSize());
            var health = generateBrickData.ParentBaseHealth;
            var heightJump = brickConfig.HeightOfJump;
            var brickId = _bricksMover.GetCurrentBrickId(true);
            
            _bricksMover.AddBrick(position.x, position.y, new FieldBrickInfo(){Id = brickId, CanMove = brick.CanMove(), IsMinion = generateBrickData.Type == BrickType.EnemyMinion});
            
            DOTween.To((value) =>
            {
                var currPosition = Vector3.Lerp(startPosititon, finishPosition, value);
                currPosition.y = Mathf.Sin(Mathf.PI * value) * heightJump;
                brick.transform.position = currPosition;
                
            }, 0, 1, .5f).OnComplete(() =>
            {
                brick.Init(generateBrickData.Type, _field.transform, new Vector3(_field.StartSpawnX + place.y * _field.GetBaseSize(), 0, _field.StartSpawnY - place.x * _field.GetBaseSize()), 
                    brickId, health, brickConfig);
                OnComplete?.Invoke(brick);
            });

            return true;
        }
        
        public async Task CreateClone(BrickBase parentBrick, BrickCloneResult clone, Action<BrickBase> onComplete, float healthCoefficient)
        {
            var place = clone.Position;
            var brick = Create(clone.Type);
            float heightJump;
            var health = parentBrick.BaseHealth;
            
            switch (clone.Type)
            {
                case BrickType.EnemyMinionCyclop:
                {
                    var brickMinionConfig = _enemiesConfig.GetConfig(clone.Type) as MinionCyclopsConfig;
                    heightJump = brickMinionConfig.HeightOfJump;
                    break;
                }
                case BrickType.EnemyMinion:
                {
                    var brickConfig = _enemiesConfig.GetConfig(clone.Type) as MinionConfig;
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BossMinionConfig;
                    heightJump = brickParentConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient * brickParentConfig.MinionHPCoefficient);
                    break;
                }
                case BrickType.EnemyRam:
                {
                    var brickConfig = _enemiesConfig.GetConfig(clone.Type);
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BossRamConfig;
                    heightJump = brickParentConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient);
                    break;
                }
                case BrickType.EnemyGhost:
                {
                    var brickConfig = _enemiesConfig.GetConfig(clone.Type);
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BossGhostConfig;
                    heightJump = brickParentConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient);
                    break;
                }
                case BrickType.EnemyStoneBox:
                {
                    var brickConfig = _enemiesConfig.GetConfig(clone.Type);
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BossStoneConfig;
                    heightJump = brickParentConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient);
                    break;
                }
                case BrickType.EnemyShieldBox:
                {
                    var brickConfig = _enemiesConfig.GetConfig(clone.Type);
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BossShieldConfig;
                    heightJump = brickParentConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient);
                    break;
                }
                case BrickType.EnemySpider:
                {
                    var brickConfig = _enemiesConfig.GetConfig(clone.Type);
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BossSpiderConfig;
                    heightJump = brickParentConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient);
                    break;
                }
                case BrickType.EnemyHealer:
                {
                    var brickConfig = _enemiesConfig.GetConfig(clone.Type);
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BossHealerConfig;
                    heightJump = brickParentConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient);
                    break;
                }
                case BrickType.BossBomber:
                {
                    var brickConfig = _enemiesConfig.GetConfig(clone.Type);
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BossBomberConfig;
                    heightJump = brickParentConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient);
                    break;
                }
                default:
                {
                    var brickConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BaseBrickConfig;
                    var brickParentConfig = _enemiesConfig.GetConfig(parentBrick.TypeOfBrick) as BaseEnemyConfig;
                    heightJump = brickConfig.HeightOfJump;
                    health =Mathf.CeilToInt( parentBrick.BaseHealth / brickParentConfig.StartHpCoefficient);
                    break;
                }
            }

            var startPosititon = parentBrick.GetPosition(_field.GetBaseSize());
            var finishPosition = new Vector3(_field.StartSpawnX + place.y * _field.GetBaseSize(), 0,
                _field.StartSpawnY - place.x * _field.GetBaseSize());

            health = (int) (health * healthCoefficient);
            
            var duration = _gameConfig.GetDefaultFieldConfig.EnemyTurnDelay;

            DOTween.To((value) =>
            {
                var currPosition = Vector3.Lerp(startPosititon, finishPosition, value);
                currPosition.y = Mathf.Sin(Mathf.PI * value) * heightJump;
                brick.transform.position = currPosition;

            }, 0, 1, duration).OnComplete(() =>
            {
                brick.Init(clone.Type, _field.transform,
                    new Vector3(_field.StartSpawnX + place.y * _field.GetBaseSize(), 0,
                        _field.StartSpawnY - place.x * _field.GetBaseSize()),
                    clone.CloneId, health, _enemiesConfig.GetConfig(clone.Type));
                onComplete?.Invoke(brick);
            });
            if (!clone.NotNeedWait)
            {
                await new WaitForSeconds(duration);
            }
        }
        
        public ExtraBall CreateNewExtraBall(Vector3 transformPosition, Vector3 endPosition)
        {
            var extraBall = (ExtraBall)Create(BrickType.ExtraBall);
            extraBall.Init(BrickType.ExtraBall, _field.transform, transformPosition, -1, 0);
            extraBall.DisableCollider();
            
            extraBall.TweenMove(endPosition, .5f, null);
            return extraBall;
        }

        public BrickBase Create(BrickType type)
        {
            BrickBase brick = type switch
            {
                BrickType.Cube => _cubeBrickPool.Spawn(),
                BrickType.Cylinder => _cylinderBrickPool.Spawn(),
                BrickType.PrismSW => _prismSWBrickPool.Spawn(),
                BrickType.PrismSE => _prismSEBrickPool.Spawn(),
                BrickType.PrismNE => _prismNEBrickPool.Spawn(),
                BrickType.PrismNW => _prismNWBrickPool.Spawn(),
                BrickType.EnemyMinion => _enemyMinionPool.Spawn(),
                BrickType.EnemyMinionCyclop => _enemyMinionCyclopPool.Spawn(),
                BrickType.EnemyGhost => _enemyGhostPool.Spawn(),
                BrickType.EnemyShieldBox => _enemyShieldPool.Spawn(),
                BrickType.EnemyStoneBox => _enemyStonePool.Spawn(),
                BrickType.EnemyCloner => _enemyClonerPool.Spawn(),
                BrickType.EnemySpecialCloner => _enemyClonerSpecialPool.Spawn(),
                BrickType.EnemySpider => _enemySpiderPool.Spawn(),
                BrickType.EnemyRam => _enemyRamPool.Spawn(),
                BrickType.EnemyHealer => _enemyHealerPool.Spawn(),
                BrickType.EnemyBomber => _enemyBomberPool.Spawn(),
                BrickType.BossMinion => _bossMinionPool.Spawn(),
                BrickType.BossRam => _bossRamPool.Spawn(),
                BrickType.BossGhost => _bossGhostPool.Spawn(),
                BrickType.BossStone => _bossStonePool.Spawn(),
                BrickType.BossShield => _bossShieldPool.Spawn(),
                BrickType.BossSpider => _bossSpiderPool.Spawn(),
                BrickType.BossCloner => _bossClonerPool.Spawn(),
                BrickType.BossClonerSpecial => _bossClonerSpecialPool.Spawn(),
                BrickType.BossHealer => _bossHealerPool.Spawn(),
                BrickType.BossBomber => _bossBomberPool.Spawn(),
                BrickType.Obstacle => _obstaclePool.Spawn(),
                BrickType.ExtraBall => _extraBallPool.Spawn(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return brick;
        }
        
        public void DestroyBrick(BrickBase brick)
        {
            switch (brick.TypeOfBrick)
            {
                case BrickType.Cube:
                    _cubeBrickPool.Despawn((CubeBrick)brick);
                    break;
                case BrickType.Cylinder:
                    _cylinderBrickPool.Despawn((CylinderBrick)brick);
                    break;
                case BrickType.PrismSW:
                    _prismSWBrickPool.Despawn((PrismBrickSW)brick);
                    break;
                case BrickType.PrismSE:
                    _prismSEBrickPool.Despawn((PrismBrickSE)brick);
                    break;
                case BrickType.PrismNE:
                    _prismNEBrickPool.Despawn((PrismBrickNE)brick);
                    break;
                case BrickType.PrismNW:
                    _prismNWBrickPool.Despawn((PrismBrickNW)brick);
                    break;
                case BrickType.EnemyMinion:
                    _enemyMinionPool.Despawn((EnemyMinion)brick);
                    break;
                case BrickType.EnemyMinionCyclop:
                    _enemyMinionCyclopPool.Despawn((EnemyMinionCyclop)brick);
                    break;
                case BrickType.EnemyGhost:
                    _enemyGhostPool.Despawn((EnemyGhost)brick);
                    break;
                case BrickType.EnemyShieldBox:
                    _enemyShieldPool.Despawn((EnemyShield)brick);
                    break;
                case BrickType.EnemyStoneBox:
                    _enemyStonePool.Despawn((EnemyStone)brick);
                    break;
                case BrickType.EnemyCloner:
                    _enemyClonerPool.Despawn((EnemyCloner)brick);
                    break;
                case BrickType.EnemySpecialCloner:
                    _enemyClonerSpecialPool.Despawn((EnemyClonerSpecial)brick);
                    break;
                case BrickType.EnemySpider:
                    _enemySpiderPool.Despawn((EnemySpider)brick);
                    break;
                case BrickType.EnemyRam:
                    _enemyRamPool.Despawn((EnemyRam)brick);
                    break;
                case BrickType.EnemyHealer:
                    _enemyHealerPool.Despawn((EnemyHealer)brick);
                    break;
                case BrickType.BossMinion:
                    _bossMinionPool.Despawn((BossMinion)brick);
                    break;
                case BrickType.BossGhost:
                    _bossGhostPool.Despawn((BossGhost)brick);
                    break;
                case BrickType.BossRam:
                    _bossRamPool.Despawn((BossRam)brick);
                    break;
                case BrickType.BossStone:
                    _bossStonePool.Despawn((BossStone)brick);
                    break;
                case BrickType.BossShield:
                    _bossShieldPool.Despawn((BossShield)brick);
                    break;
                case BrickType.BossSpider:
                    _bossSpiderPool.Despawn((BossSpider)brick);
                    break;
                case BrickType.BossCloner:
                    _bossClonerPool.Despawn((BossCloner)brick);
                    break;
                case BrickType.BossClonerSpecial:
                    _bossClonerSpecialPool.Despawn((BossClonerSpecial)brick);
                    break;
                case BrickType.BossHealer:
                    _bossHealerPool.Despawn((BossHealer)brick);
                    break;
                case BrickType.BossBomber:
                    _bossBomberPool.Despawn((BossBomber)brick);
                    break;
                case BrickType.Obstacle:
                    _obstaclePool.Despawn((Obstacle)brick);
                    break;
                case BrickType.ExtraBall:
                    _extraBallPool.Despawn((ExtraBall)brick);
                    break;
                case BrickType.EnemyBomber:
                    _enemyBomberPool.Despawn((EnemyBomber)brick);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}