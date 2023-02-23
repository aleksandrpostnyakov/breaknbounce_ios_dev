using System;
using Config;
using Sound;
using UI;

namespace StateMashine
{
    public class WinState : IState
    {
        private WinScreen _winScreen;
        private GameConfig _gameConfig;
        private PlayerInfo.PlayerGameInfo _playerGameInfo;
        private ISoundManager _soundManager;
        private int _winCoins;
        
        public WinState(UiService uiService, GameConfig gameConfig, PlayerInfo.PlayerGameInfo playerGameInfo, ISoundManager soundManager)
        {
            _gameConfig = gameConfig;
            _playerGameInfo = playerGameInfo;
            _soundManager = soundManager;
            
            _winScreen = uiService.Get<WinScreen>();
            _winScreen.OnPlay += WinScreenOnPlay;
            _winScreen.OnLottery += WinScreenOnLottery;
        }
        
        private void WinScreenOnPlay()
        {
            _playerGameInfo.AddCoins(_winCoins);
            NextState?.Invoke(StateMashineStateType.Upgrade);
        }
        
        private void WinScreenOnLottery(float mult)
        {
            _playerGameInfo.AddCoins((int)(_winCoins * mult));
            _soundManager.PlayUISound(UISoundId.CoinAdd);
            NextState?.Invoke(StateMashineStateType.Upgrade);
        }
        
        public void Enter()
        {
            _playerGameInfo.EnterInRewardScreen(true);
            var coinsConfig = _gameConfig.GetCoinsConfig;
            _winCoins = coinsConfig.StartReward + coinsConfig.RewardIncrement * (_playerGameInfo.CurrentLevel - 2);
            _winScreen.Show(_winCoins);
        }

        public void Exit()
        {
            _playerGameInfo.EnterInRewardScreen(false);
            _winScreen.Hide();
        }

        public event Action<StateMashineStateType> NextState;
    }
}