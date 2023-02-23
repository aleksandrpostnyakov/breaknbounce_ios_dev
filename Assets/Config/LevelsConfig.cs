using System;
using System.Linq;
using Items;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "Levels", menuName = "Config/Levels", order = 1)]
    public class LevelsConfig : ScriptableObject
    {
        [SerializeField] private LevelConfig[] _levels;

        public int CountOfLevels => _levels.Length;

        public LevelConfig GetLevel(int id)
        {
            if (id > 100)
            {
                id = (id - 10) % 90 + 10;
            }
            return _levels.FirstOrDefault(l => l.LevelId == id);
        }
    }

    [Serializable]
    public class LevelConfig
    {
        public int LevelId;
        public int CountOfWaves;
        public int StartObstacleHP;
        public int IncrementObstacleHP;
        public int MinCountOfObstacles;
        public float ChanсeOfEnemies;
        public BrickType[] EnemyTypes;
        public BrickType Boss;
        public BrickType MiddleBoss = BrickType.None;
        public int MiddleBossWave;
        public TutorialBoss TutorialBoss;
        public Vector2Int[] Obstacles;
        public int BonusBalls;
        public int ExtraBonus;
        public int NumberOfRows;
        public int NumberOfCols;
        public float StartChanсeOfEnemies;
        public float DifficultyDecrement;

        public LevelConfig Clone()
        {
            var clone = new LevelConfig()
            {
                LevelId = LevelId,
                CountOfWaves = CountOfWaves,
                StartObstacleHP = StartObstacleHP,
                IncrementObstacleHP = IncrementObstacleHP,
                MinCountOfObstacles = MinCountOfObstacles,
                ChanсeOfEnemies = ChanсeOfEnemies,
                EnemyTypes = EnemyTypes,
                Boss = Boss,
                MiddleBoss = MiddleBoss,
                MiddleBossWave = MiddleBossWave,
                TutorialBoss = TutorialBoss,
                Obstacles = Obstacles,
                BonusBalls = BonusBalls,
                ExtraBonus = ExtraBonus,
                NumberOfRows = NumberOfRows,
                NumberOfCols = NumberOfCols,
                StartChanсeOfEnemies = StartChanсeOfEnemies,
                DifficultyDecrement = DifficultyDecrement,
            };

            return clone;
        }
    }

    [Serializable]
    public class TutorialBoss
    {
        public float TutorialBossHP = 1;
        public float TutorialBossDamage = 1;
    }
}