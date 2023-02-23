using System;
using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Config/Game", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("SKILLS")]
        [SerializeField] private SkillConfig _shieldSkillConfig;
        [SerializeField] private SkillConfig _dotsSkillConfig;
        [SerializeField] private SkillConfig _ballsSkillConfig;
        [SerializeField] private CoinsConfig _coinsConfig;
        [SerializeField] private float _lotterySpeed;


        [Header("FIELD")]
        [SerializeField] private DefaulFieldConfig  _defaultFieldConfig;
        
        [Header("HUD")]
        [SerializeField] private HudConfig  _hudConfig;
        [SerializeField] private int _adsDelay;
        
        [Header("TEST MODE")]
        [SerializeField] private bool _testMode;
        [SerializeField] private TestModeConfig _testModeConfig;

        public SkillConfig GetShieldConfig => _shieldSkillConfig;
        public SkillConfig GetDotsConfig => _dotsSkillConfig;
        public SkillConfig GetBallsConfig => _ballsSkillConfig;
        public CoinsConfig GetCoinsConfig => _coinsConfig;
        public DefaulFieldConfig GetDefaultFieldConfig => _defaultFieldConfig;
        public HudConfig GetHudConfig => _hudConfig;

        public float LotterySpeed => _lotterySpeed;
        public float AdsDelay => _adsDelay;

        public TestModeConfig GetTestModeConfig()
        {
            return _testMode ? _testModeConfig : null;
        }
    }
    
    [Serializable]
    public class CoinsConfig
    {
        public int StartBalance;
        public int StartReward;
        public int RewardIncrement;
        public float ConsolationPrize;
    }

    [Serializable]
    public class SkillConfig
    {
        public string Name;
        public int StartValue;
        public int IncrementStep;
        public int StartCostOfUpgrade;
        public CostIncreaseCoefficient[] CostIncreaseCoefficients;
    }

    [Serializable]
    public class CostIncreaseCoefficient
    {
        public int Level;
        public int Cost;
    }
    
    [Serializable]
    public class DefaulFieldConfig
    {
        public int DefaultFieldRows;
        public int DefaultFieldCols;
        public float FieldGenerationSpeed;
        public float DotScale;
        public float BetweenDots;
        public float BallStartDelay;
        public float BallSpeedAddiction;
        public float oversize2D;
        public float oversize3D;
        public float HitScaleCoefficient;
        public float HitScaleTime;
        public float BulletFlyTime;
        public float CannonBanAngle;
        public float HealFlyTime;
        public float AimOffset;
        public float EnemyTurnDelay;
    }
    
    [Serializable]
    public class TestModeConfig
    {
        public int StartLevel;
        public int Shield;
        public int Dots;
        public int Balls;
        public float Coins;
    }
    
    [Serializable]
    public class HudConfig
    {
        [SerializeField] public float KillBallsButtonShowDelay;
        [SerializeField] public float RewardsMultiplier1;
        [SerializeField] public float RewardsMultiplier2;
        [SerializeField] public float RewardsMultiplier3;
    }
    
}