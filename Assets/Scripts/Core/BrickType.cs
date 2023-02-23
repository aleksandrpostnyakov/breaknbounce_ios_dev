using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public enum BrickType
    {
        None = 0,
        Cube = 1,
        Cylinder = 2,
        PrismSW = 3,
        PrismSE = 4,
        PrismNE = 5,
        PrismNW = 6,
        EnemyMinion = 10,
        EnemyMinionCyclop = 11,
        EnemyRam = 12,
        EnemySpider = 13,
        EnemyGhost = 14,
        EnemyStoneBox = 15,
        EnemyShieldBox = 16,
        EnemyCloner = 17,
        EnemySpecialCloner = 18,
        EnemyHealer = 19,
        EnemyBomber = 20,
        BossMinion = 100,
        BossRam = 1001,
        BossGhost = 1002,
        BossStone = 1003,
        BossShield = 1004,
        BossSpider = 1005,
        BossCloner = 1006,
        BossClonerSpecial = 1007,
        BossHealer = 1008,
        BossBomber = 1009,
        Obstacle = 200,
        ExtraBall = 300
    }
    
    public static class BrickTypeHelper
    {
        private static readonly List<BrickType> common = new()
            {BrickType.Cube};

        private static readonly List<BrickType> special = new()
            {BrickType.Cylinder, BrickType.PrismNE, BrickType.PrismNW, BrickType.PrismSE, BrickType.PrismSW};
        
        private static readonly List<BrickType> enemies = new()
        {
            BrickType.EnemyMinion, BrickType.EnemyMinionCyclop, BrickType.EnemyRam, BrickType.EnemySpider, BrickType.EnemyGhost,
            BrickType.EnemyStoneBox, BrickType.EnemyShieldBox, BrickType.EnemyCloner, BrickType.EnemySpecialCloner, BrickType.EnemyHealer,
            BrickType.EnemyBomber
        };
        
        private static readonly List<BrickType> bosses = new()
        {
            BrickType.BossMinion, BrickType.BossRam, BrickType.BossGhost, BrickType.BossStone, BrickType.BossShield,
            BrickType.BossSpider, BrickType.BossCloner, BrickType.BossClonerSpecial, BrickType.BossHealer, BrickType.BossBomber
        };
        
        private static readonly List<BrickType> jumpingBosses = new()
        {
            BrickType.BossSpider, BrickType.BossCloner, BrickType.BossClonerSpecial, BrickType.BossHealer, BrickType.BossBomber
        };
        
        private static readonly List<BrickType> bonuses = new()
        {
            BrickType.ExtraBall
        };
        
        public static BrickType GetRandomSpecialType()
        {
            return special[Random.Range(0, special.Count)];
        }
        
        public static BrickType GetRandomCommonType()
        {
            return common[Random.Range(0, common.Count)];
        }
        
        public static bool IsCommon(BrickType type)
        {
            return common.Contains(type) || special.Contains(type);
        }

        public static bool IsEnemy(BrickType type)
        {
            return enemies.Contains(type);
        }
        
        public static bool IsBoss(BrickType type)
        {
            return bosses.Contains(type);
        }
        
        public static bool IsJumpingBoss(BrickType type)
        {
            return jumpingBosses.Contains(type);
        }
        public static bool IsBonus(BrickType type)
        {
            return bonuses.Contains(type);
        }
        
    }
}