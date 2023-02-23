using System;
using UI;

namespace StateMashine
{
    public class SettingsState : IState
    {
        private SettingsScreen _settingsScreen;
        
        public SettingsState(UiService uiService)
        {
            _settingsScreen = uiService.Get<SettingsScreen>();
            _settingsScreen.OnExit += SettingsScreenOnExit;
        }
        
        private void SettingsScreenOnExit()
        {
            NextState?.Invoke(StateMashineStateType.Upgrade);
        }
        
        public void Enter()
        {
            _settingsScreen.Show();
        }

        public void Exit()
        {
            _settingsScreen.Hide();
        }

        public event Action<StateMashineStateType> NextState;
    }
}