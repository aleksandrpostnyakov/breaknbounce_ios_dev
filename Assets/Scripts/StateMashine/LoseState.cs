using System;
using UI;

namespace StateMashine
{
    public class LoseState : IState
    {
        private LoseScreen _loseScreen;
        public LoseState(UiService uiService)
        {
            _loseScreen = uiService.Get<LoseScreen>();
            _loseScreen.OnPlay += LoseScreenOnPlay;
            _loseScreen.OnHome += LoseScreenOnHome;
        }
        
        private void LoseScreenOnPlay()
        {
            NextState?.Invoke(StateMashineStateType.Level);
        }
        
        private void LoseScreenOnHome()
        {
            NextState?.Invoke(StateMashineStateType.Upgrade);
        }
        
        public void Enter()
        {
            _loseScreen.Show();
        }

        public void Exit()
        {
            _loseScreen.Hide();
        }

        public event Action<StateMashineStateType> NextState;
    }
}