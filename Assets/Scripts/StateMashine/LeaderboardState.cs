using System;
using UI;

namespace StateMashine
{
    public class LeaderboardState : IState
    {
        private LeaderboardScreen _leaderboardScreen;
        
        public LeaderboardState(UiService uiService)
        {
            _leaderboardScreen = uiService.Get<LeaderboardScreen>();
            _leaderboardScreen.OnExit += LeaderboardScreenOnExit;
        }
        
        private void LeaderboardScreenOnExit()
        {
            NextState?.Invoke(StateMashineStateType.Upgrade);
        }
        
        public void Enter()
        {
            _leaderboardScreen.Show();
        }

        public void Exit()
        {
            _leaderboardScreen.Hide();
        }

        public event Action<StateMashineStateType> NextState;
    }
}