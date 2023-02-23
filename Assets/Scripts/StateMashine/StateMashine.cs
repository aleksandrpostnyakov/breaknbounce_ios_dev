using System;
using System.Collections.Generic;
using Config;
using Level;
using Sound;
using UI;
using UpgradeState;
using Zenject;

namespace StateMashine
{
    public enum StateMashineStateType
    {
        Loading,
        Upgrade,
        Level,
        Win,
        Lose,
        Settings,
        Leaderboard
    }
    
    public class StateMashine
    {
        [Inject] private GameLoader.GameLoader _gameLoader;
        [Inject] private UserUpgrader _userUpgrader;
        [Inject] private PlayerInfo.PlayerGameInfo _playerGameInfo;
        [Inject] private UiService _uiService;
        [Inject] private GameConfig _gameConfig;
        [Inject] private Tutorial _tutorial;
        [Inject] private IAdsSystem _adsSystem;
        [Inject] private ISoundManager _soundManager;
        
        private IState _currentState;
        private Dictionary<StateMashineStateType, IState> _states;

        public StateMashine(GameLoader.GameLoader gameLoader, UserUpgrader userUpgrader, PlayerInfo.PlayerGameInfo playerGameInfo, ISoundManager soundManager)
        {
            _gameLoader = gameLoader;
            _userUpgrader = userUpgrader;
            _playerGameInfo = playerGameInfo;
            _soundManager = soundManager;
        }
        
        public void Initalize()
        {
            _states = new Dictionary<StateMashineStateType, IState>();
            SetState(StateMashineStateType.Loading);
        }

        private void SetState(StateMashineStateType stateType)
        {
            if (_currentState != null)
            {
                _currentState.NextState -= OnNextState;
            }
            _currentState?.Exit();
            _currentState = _states.ContainsKey(stateType) ? _states[stateType] : CreateState(stateType);
            
            _currentState.NextState += OnNextState;
            _currentState.Enter();
            
        }

        private void OnNextState(StateMashineStateType state)
        {
            SetState(state);
        }

        private IState CreateState(StateMashineStateType stateType)
        {
            IState state = stateType switch
            {
                StateMashineStateType.Loading => new LoadingState(_gameLoader, _playerGameInfo, _tutorial, _adsSystem),
                StateMashineStateType.Upgrade => new UpgradeState(_userUpgrader),
                StateMashineStateType.Level => new LevelState(_gameLoader),
                StateMashineStateType.Lose => new LoseState(_uiService),
                StateMashineStateType.Win => new WinState(_uiService, _gameConfig, _playerGameInfo, _soundManager),
                StateMashineStateType.Settings => new SettingsState(_uiService),
                StateMashineStateType.Leaderboard => new LeaderboardState(_uiService),
                _ => throw new ArgumentOutOfRangeException(nameof(stateType), stateType, null)
            };

            _states.Add(stateType, state);
            
            return state;
        }
    }
}