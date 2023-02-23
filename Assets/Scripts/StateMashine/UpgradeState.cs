using System;
using UpgradeState;
using Zenject;

namespace StateMashine
{
    public class UpgradeState : IState
    {
        private UserUpgrader _upgrader;
        public event Action<StateMashineStateType> NextState;

        public UpgradeState(UserUpgrader upgrader)
        {
            _upgrader = upgrader;
            _upgrader.StartLevel += UpgraderOnStartLevel;
            _upgrader.ShowSettings += UpgraderOnShowSettings;
            _upgrader.ShowLeaderboard += UpgraderOnShowLeaderboard;
        }
        
        private void UpgraderOnShowLeaderboard()
        {
            NextState?.Invoke(StateMashineStateType.Leaderboard);
        }

        private void UpgraderOnShowSettings()
        {
            NextState?.Invoke(StateMashineStateType.Settings);
        }

        private void UpgraderOnStartLevel()
        {
            NextState?.Invoke(StateMashineStateType.Level);
        }

        public void Enter()
        {
            _upgrader.Initialize();
        }

        public void Exit()
        {
            _upgrader.Hide();
        }
    }
}