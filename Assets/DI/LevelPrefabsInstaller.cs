using Items;
using Level;
using UnityEngine;
using Zenject;

public class LevelPrefabsInstaller : MonoInstaller
{
    [SerializeField] private Ball _ballPrefab;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private FieldCell _fieldCellPrefab;
    
    [SerializeField] private CubeBrick _cubeBrickPrefab;
    [SerializeField] private CylinderBrick _cylinderBrickPrefab;
    [SerializeField] private PrismBrickNE _prismNEBrickPrefab;
    [SerializeField] private PrismBrickNW _prismNWBrickPrefab;
    [SerializeField] private PrismBrickSE _prismSEBrickPrefab;
    [SerializeField] private PrismBrickSW _prismSWBrickPrefab;
    [SerializeField] private EnemyMinion _enemyMinionPrefab;
    [SerializeField] private EnemyMinionCyclop _enemyMinionCyclopPrefab;
    [SerializeField] private EnemyGhost _enemyGhostPrefab;
    [SerializeField] private EnemyShield _enemyShieldPrefab;
    [SerializeField] private EnemyStone _enemyStonePrefab;
    [SerializeField] private EnemyCloner _enemyClonerPrefab;
    [SerializeField] private EnemyClonerSpecial _enemyClonerSpecialPrefab;
    [SerializeField] private EnemySpider _enemySpiderPrefab;
    [SerializeField] private EnemyRam _enemyRamPrefab;
    [SerializeField] private EnemyHealer _enemyHealerPrefab;
    [SerializeField] private EnemyBomber _enemyBomberPrefab;
    [SerializeField] private BossMinion _bossMinonPrefab;
    [SerializeField] private BossRam _bossRamPrefab;
    [SerializeField] private BossGhost _bossGhostPrefab;
    [SerializeField] private BossStone _bossStonePrefab;
    [SerializeField] private BossShield _bossShieldPrefab;
    [SerializeField] private BossSpider _bossSpiderPrefab;
    [SerializeField] private BossCloner _bossClonerPrefab;
    [SerializeField] private BossClonerSpecial _bossClonerSpecialPrefab;
    [SerializeField] private BossHealer _bossHealerPrefab;
    [SerializeField] private BossBomber _bossBomberPrefab;
    [SerializeField] private Obstacle _obstaclePrefab;
    [SerializeField] private ExtraBall _extraBallPrefab;
    [SerializeField] private Dot _dotPrefab;
    [SerializeField] private WarningIcon _warningIconPrefab;
    public override void InstallBindings()
    {
        Container.BindMemoryPool<FieldCell, FieldCell.Pool>().FromComponentInNewPrefab(_fieldCellPrefab);
        Container.BindMemoryPool<Ball, Ball.Pool>().FromComponentInNewPrefab(_ballPrefab);
        Container.BindMemoryPool<Bullet, Bullet.Pool>().FromComponentInNewPrefab(_bulletPrefab);
        Container.BindMemoryPool<CubeBrick, CubeBrick.Pool>().FromComponentInNewPrefab(_cubeBrickPrefab);
        Container.BindMemoryPool<CylinderBrick, CylinderBrick.Pool>().FromComponentInNewPrefab(_cylinderBrickPrefab);
        Container.BindMemoryPool<PrismBrickNE, PrismBrickNE.Pool>().FromComponentInNewPrefab(_prismNEBrickPrefab);
        Container.BindMemoryPool<PrismBrickNW, PrismBrickNW.Pool>().FromComponentInNewPrefab(_prismNWBrickPrefab);
        Container.BindMemoryPool<PrismBrickSE, PrismBrickSE.Pool>().FromComponentInNewPrefab(_prismSEBrickPrefab);
        Container.BindMemoryPool<PrismBrickSW, PrismBrickSW.Pool>().FromComponentInNewPrefab(_prismSWBrickPrefab);
        Container.BindMemoryPool<EnemyMinion, EnemyMinion.Pool>().FromComponentInNewPrefab(_enemyMinionPrefab);
        Container.BindMemoryPool<EnemyMinionCyclop, EnemyMinionCyclop.Pool>().FromComponentInNewPrefab(_enemyMinionCyclopPrefab);
        Container.BindMemoryPool<EnemyGhost, EnemyGhost.Pool>().FromComponentInNewPrefab(_enemyGhostPrefab);
        Container.BindMemoryPool<EnemyShield, EnemyShield.Pool>().FromComponentInNewPrefab(_enemyShieldPrefab);
        Container.BindMemoryPool<EnemyStone, EnemyStone.Pool>().FromComponentInNewPrefab(_enemyStonePrefab);
        Container.BindMemoryPool<EnemyCloner, EnemyCloner.Pool>().FromComponentInNewPrefab(_enemyClonerPrefab);
        Container.BindMemoryPool<EnemyClonerSpecial, EnemyClonerSpecial.Pool>().FromComponentInNewPrefab(_enemyClonerSpecialPrefab);
        Container.BindMemoryPool<EnemySpider, EnemySpider.Pool>().FromComponentInNewPrefab(_enemySpiderPrefab);
        Container.BindMemoryPool<EnemyRam, EnemyRam.Pool>().FromComponentInNewPrefab(_enemyRamPrefab);
        Container.BindMemoryPool<EnemyHealer, EnemyHealer.Pool>().FromComponentInNewPrefab(_enemyHealerPrefab);
        Container.BindMemoryPool<EnemyBomber, EnemyBomber.Pool>().FromComponentInNewPrefab(_enemyBomberPrefab);
        Container.BindMemoryPool<BossMinion, BossMinion.Pool>().FromComponentInNewPrefab(_bossMinonPrefab);
        Container.BindMemoryPool<BossRam, BossRam.Pool>().FromComponentInNewPrefab(_bossRamPrefab);
        Container.BindMemoryPool<BossGhost, BossGhost.Pool>().FromComponentInNewPrefab(_bossGhostPrefab);
        Container.BindMemoryPool<BossStone, BossStone.Pool>().FromComponentInNewPrefab(_bossStonePrefab);
        Container.BindMemoryPool<BossShield, BossShield.Pool>().FromComponentInNewPrefab(_bossShieldPrefab);
        Container.BindMemoryPool<BossSpider, BossSpider.Pool>().FromComponentInNewPrefab(_bossSpiderPrefab);
        Container.BindMemoryPool<BossCloner, BossCloner.Pool>().FromComponentInNewPrefab(_bossClonerPrefab);
        Container.BindMemoryPool<BossClonerSpecial, BossClonerSpecial.Pool>().FromComponentInNewPrefab(_bossClonerSpecialPrefab);
        Container.BindMemoryPool<BossHealer, BossHealer.Pool>().FromComponentInNewPrefab(_bossHealerPrefab);
        Container.BindMemoryPool<BossBomber, BossBomber.Pool>().FromComponentInNewPrefab(_bossBomberPrefab);
        Container.BindMemoryPool<Obstacle, Obstacle.Pool>().FromComponentInNewPrefab(_obstaclePrefab);
        Container.BindMemoryPool<ExtraBall, ExtraBall.Pool>().FromComponentInNewPrefab(_extraBallPrefab);
        Container.BindMemoryPool<Dot, Dot.Pool>().FromComponentInNewPrefab(_dotPrefab);
        Container.BindMemoryPool<WarningIcon, WarningIcon.Pool>().FromComponentInNewPrefab(_warningIconPrefab);
    }
}