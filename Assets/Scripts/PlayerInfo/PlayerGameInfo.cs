using System;
using Config;
using Level;
using Zenject;

namespace PlayerInfo
{
    public class PlayerGameInfo : IDisposable
    {
        private GameConfig _gameConfig;
        private Tutorial _tutorial;
        
        private PlayerData _data;
        private SettingsData _settingsData;

        public event Action<bool> Changed;
        public event Action XPChanged;
        public event Action CoinsChanged;
        public event Action<bool> SettingsChanged;

        public PlayerGameInfo(GameConfig gameConfig, Tutorial tutorial)
        {
            _gameConfig = gameConfig;
            _tutorial = tutorial;
        }

        public void Initialize(PlayerData data)
        {
            _data = data;
        }
        
        public void InitializeSettings(SettingsData data)
        {
            if (data == null)
            {
                _settingsData = new SettingsData()
                {
                    PrivacyAccepted = false,
                    RateStars = 0,
                    UpgradeIds = 123,
                    UpgradeAdsTime = new DateTime()
                };
            }
            else
            {
                _settingsData = data;
                _tutorial.SetStady(_settingsData.TutorialStady);
            }
            
            _tutorial.Changed += TutorialChanged;
        }

        public void Initialize()
        {
            _data = new PlayerData()
            {
                HealthLevel = 0,
                DotsLevel = 0,
                BallsLevel = 0,
                Coins = _gameConfig.GetCoinsConfig.StartBalance
            };
        }

        public int GetHealth(int testValue = 0)
        {
            var shieldConfig = _gameConfig.GetShieldConfig;
            var healthLevel = testValue == 0 ? _data.HealthLevel : testValue;
            return shieldConfig.StartValue + shieldConfig.IncrementStep * healthLevel;
        }
        
        public int GetDots()
        {
            var dotsConfig = _gameConfig.GetDotsConfig;
            return dotsConfig.StartValue + dotsConfig.IncrementStep * _data.DotsLevel;
        }
        
        public int GetBalls()
        {
            var ballsConfig = _gameConfig.GetBallsConfig;
            return ballsConfig.StartValue + ballsConfig.IncrementStep * _data.BallsLevel;
        }

        public int GetHealthLevel => _data.HealthLevel;
        public int GetDotsLevel => _data.DotsLevel;
        public int GetBallsLevel => _data.BallsLevel;
        public int GetCoins => _data.Coins;
        public int GetXP => _data.Xp;
        public int GetOldXP => _data.OldXp;
        public int GetLevelLoseCount => _data.LevelLoseCount;
        public int CurrentLevel => _data.GameLevel;
        public PlayerData Data => _data;
        public SettingsData SettingsData => _settingsData;

        public void IncreaseHealthLevel()
        {
            _data.HealthLevel++;
            Changed?.Invoke(true);
        }
        
        public void IncreaseDotsLevel()
        {
            _data.DotsLevel++;
            Changed?.Invoke(true);
        }
        
        public void IncreaseBallsLevel()
        {
            _data.BallsLevel++;
            Changed?.Invoke(true);
        }
        
        public void IncreaseGameLevel(int level)
        {
            _data.GameLevel =  level;
            _data.LevelLoseCount = 0;
            Changed?.Invoke(true);
        }
        
        public void AddCoins(int value)
        {
            _data.Coins += value;
            CoinsChanged?.Invoke();
            Changed?.Invoke(true);
        }
        
        public void AddXP(int value)
        {
            _data.Xp += value;
            XPChanged?.Invoke();
        }
        
        public void UpdateOldXp()
        {
            _data.OldXp = _data.Xp;
        }
        
        public void IncreaseLevelLoseCount()
        {
            _data.LevelLoseCount++;
            Changed?.Invoke(true);
        }
        
        public bool TakeCoins(int value)
        {
            if (_data.Coins < value)
            {
                return false;
            }
            _data.Coins -= value;
            CoinsChanged?.Invoke();
            Changed?.Invoke(true);
            return true;
        }

        public void Rate(int rating)
        {
            _settingsData.RateStars = rating;
            SettingsChanged?.Invoke(true);
        }
        
        public void PrivacyAccept()
        {
            _settingsData.PrivacyAccepted = true;
            SettingsChanged?.Invoke(true);
        }
        
        public void AdsDisable()
        {
            _settingsData.AdsDisabled = true;
            SettingsChanged?.Invoke(true);
        }
        
        private void TutorialChanged(int value)
        {
            _settingsData.TutorialStady = value;
            SettingsChanged?.Invoke(true);
        }
        
        public void ChangeScreenTo2D(bool to2D)
        {
            if (_settingsData.ScreenIn2D == to2D)
            {
                return;
            }
            _settingsData.ScreenIn2D = to2D;
            SettingsChanged?.Invoke(true);
        }

        public void ChangeUpgradeAds(int adsId)
        {
            var currIds = _settingsData.UpgradeIds;
            var first = currIds / 100;
            var second = (currIds - first * 100) / 10;
            var third = (currIds - second * 10);

            if (first == adsId)
            {
                _settingsData.UpgradeIds = second * 100 + third * 10 + first;
            }
            else if(second == adsId)
            {
                _settingsData.UpgradeIds = first * 100 + third * 10 + second;
                
            }

            _settingsData.UpgradeAdsTime = DateTime.Now;
            SettingsChanged?.Invoke(true);
        }

        public void ClearUpgradeAdsDelay()
        {
            _settingsData.UpgradeAdsTime = new DateTime();
        }
        
        public void EnterInRewardScreen(bool enter)
        {
            _settingsData.EnterInRewardScreen = enter;
            SettingsChanged?.Invoke(true);
        }

        public void Dispose()
        {
            _tutorial.Changed -= TutorialChanged;
        }
    }

    public class PlayerData
    {
        public int GameLevel = 1;
        public int HealthLevel;
        public int DotsLevel;
        public int BallsLevel;
        public int Coins;
        public int Xp;
        public int OldXp;
        public int LevelLoseCount;
    }

    public class SettingsData
    {
        public bool PrivacyAccepted;
        public int RateStars;
        public bool NeedShowRateUs;
        public bool AdsDisabled;
        public int UpgradeIds;
        public DateTime UpgradeAdsTime;
        public int TutorialStady;
        public bool ScreenIn2D;
        public bool EnterInRewardScreen;
    }
}