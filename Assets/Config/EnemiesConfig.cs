using System;
using Items;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "EnemiesConfig", menuName = "Config/EnemiesConfig", order = 1)]
    public class EnemiesConfig : ScriptableObject
    {
        public BaseBrickConfig _baseBrickConfig;
        public MinionConfig _minion;
        public MinionCyclopsConfig minionCyclops;
        public BaseEnemyConfig _ram;
        public SpiderConfig _spider;
        public BaseEnemyConfig _ghost;
        public BaseEnemyConfig _stoneBox;
        public BaseEnemyConfig _shieldBox;
        public ClonerConfig _cloner;
        public ClonerConfig _specialCloner;
        public HealerConfig _healerConfig;
        public BomberConfig _bomberConfig;
        public BossMinionConfig _bossMinionConfig;
        public BossRamConfig _bossRamConfig;
        public BossGhostConfig _bossGhostConfig;
        public BossStoneConfig _bossStoneConfig;
        public BossShieldConfig _bossShieldConfig;
        public BossSpiderConfig _bossSpiderConfig;
        public BossClonerConfig _bossClonerConfig;
        public BossClonerSpecialConfig _BossClonerSpecialConfig;
        public BossHealerConfig _BossHealerConfig;
        public BossBomberConfig _BossBomberConfig;

        public BaseBrickConfig GetConfig(BrickType type)
        {
            var config = type switch
            {
                BrickType.EnemyMinion => _minion,
                BrickType.EnemyMinionCyclop => minionCyclops,
                BrickType.EnemyRam => _ram,
                BrickType.EnemySpider => _spider,
                BrickType.EnemyGhost => _ghost,
                BrickType.EnemyStoneBox => _stoneBox,
                BrickType.EnemyShieldBox => _shieldBox,
                BrickType.EnemyCloner => _cloner,
                BrickType.EnemySpecialCloner => _specialCloner,
                BrickType.EnemyHealer => _healerConfig,
                BrickType.EnemyBomber => _bomberConfig,
                BrickType.BossMinion => _bossMinionConfig,
                BrickType.BossRam => _bossRamConfig,
                BrickType.BossGhost => _bossGhostConfig,
                BrickType.BossStone => _bossStoneConfig,
                BrickType.BossShield => _bossShieldConfig,
                BrickType.BossSpider => _bossSpiderConfig,
                BrickType.BossCloner => _bossClonerConfig,
                BrickType.BossClonerSpecial => _BossClonerSpecialConfig,
                BrickType.BossHealer => _BossHealerConfig,
                BrickType.BossBomber => _BossBomberConfig,
                _ => _baseBrickConfig
            };

            return config;
        }
    }

    [Serializable]
    public class BaseBrickConfig
    {
        public int Priority;
        public float HeightOfJump;
        public Color BaseColor1;
        public Color BaseColor2;
    }

    [Serializable]
    public class BaseEnemyConfig: BaseBrickConfig
    {
        public float StartHpCoefficient = 1;
        public float DamageCoefficient = 1;
    }
    
    [Serializable]
    public class MinionConfig : BaseEnemyConfig
    {
    }

    [Serializable]
    public class MinionCyclopsConfig : BaseEnemyConfig
    {
    }
    
    [Serializable]
    public class SpiderConfig : BaseEnemyConfig
    {
        public float CountOfJump;
    }
    
    [Serializable]
    public class ClonerConfig : BaseEnemyConfig
    {
        public float CloneStartHpCoefficient = 1;
        public float CloneDamageCoefficient = 1;
        public int StepOfClone;
    }
    
    [Serializable]
    public class HealerConfig : BaseEnemyConfig
    {
        public float PercentOfHealing;
    }
    
    [Serializable]
    public class BomberConfig : BaseEnemyConfig
    {
        public float PercentOfDamage;
        public float ExplodeTime;
    }
    
    [Serializable]
    public class BossMinionConfig : BaseEnemyConfig
    {
        public int StepOfClone;
        public int LifeGeneration;
        public int DeathGeneration;
        public float MinionHPCoefficient;
    }
    
    [Serializable]
    public class BossRamConfig : BaseEnemyConfig
    {
        public int StepOfClone;
        public int LifeGeneration;
    }
    
    [Serializable]
    public class BossGhostConfig : BaseEnemyConfig
    {
        public int LifeGeneration;
    }
    
    [Serializable]
    public class BossStoneConfig : BaseEnemyConfig
    {
        public int LifeGeneration;
    }
    
    [Serializable]
    public class BossShieldConfig : BaseEnemyConfig
    {
        public int StepOfClone;
        public int LifeGeneration;
    }
    
    [Serializable]
    public class BossSpiderConfig : BaseEnemyConfig
    {
        public int LifeGeneration;
    }
    
    [Serializable]
    public class BossClonerConfig : BaseEnemyConfig
    {
        public int LifeGeneration;
    }
    
    [Serializable]
    public class BossClonerSpecialConfig : BaseEnemyConfig
    {
        public int LifeGeneration;
    }

    [Serializable]
    public class BossHealerConfig : BaseEnemyConfig
    {
        public int CountOfHealing;
        public int LifeGeneration;
        public float PercentOfHealing;
    }

    [Serializable]
    public class BossBomberConfig : BomberConfig
    {
        public int LifeGeneration;
    }
}