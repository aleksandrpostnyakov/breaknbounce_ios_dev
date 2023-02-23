using System;
using UI;
using Zenject;

namespace UpgradeState
{
    public class UserUpgrader
    {
        [Inject] private UiService _uiService;
        [Inject] private PlayerInfo.PlayerGameInfo _playerGameInfo;

        private UpgradeScreen _upgradeScreen;
        public event Action StartLevel;
        public event Action ShowSettings;
        public event Action ShowLeaderboard;
        private bool _isInitalized;

        public void Initialize()
        {
            if (!_isInitalized)
            {
                _upgradeScreen = _uiService.Get<UpgradeScreen>();
                _upgradeScreen.OnPlay += UpgradeScreenOnOnPlay;
                _upgradeScreen.OnSettings += UpgradeScreenOnOnSettings;
                _upgradeScreen.OnLeaderboard += UpgradeScreenOnOnLeaderboard;
                _isInitalized = true;
            }
            
            _upgradeScreen.Show();
        }

        private void UpgradeScreenOnOnSettings()
        {
            ShowSettings?.Invoke();
        }
        
        private void UpgradeScreenOnOnLeaderboard()
        {
            ShowLeaderboard?.Invoke();
        }

        private void UpgradeScreenOnOnPlay()
        {
            StartLevel?.Invoke();
        }

        public void Hide()
        {
            _upgradeScreen.Hide();
        }
    }
}