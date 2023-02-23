using System;
using Level;
using UnityEngine;
using Zenject;

namespace StateMashine
{
    public class LoadingState: IState
    {
        private GameLoader.GameLoader _gameLoader;
        private PlayerInfo.PlayerGameInfo _playerGameInfo;
        private Tutorial _tutorial;
        private IAdsSystem _adsSystem;

        public LoadingState(GameLoader.GameLoader gameLoader, PlayerInfo.PlayerGameInfo playerGameInfo, Tutorial tutorial, IAdsSystem adsSystem)
        {
            _gameLoader = gameLoader;
            _playerGameInfo = playerGameInfo;
            _tutorial = tutorial;
            _adsSystem = adsSystem;
        }
        
        public void Enter()
        {
            _gameLoader.GameStarted += GameLoaderOnGameStarted;
            _gameLoader.Initialize();
            _playerGameInfo.Initialize();
        }

        private void GameLoaderOnGameStarted()
        {
            if (!_tutorial.IsFinished())
            {
                if ((int)_tutorial.GetStady() < (int)TutorialStady.WinFirstLevel)
                {
                    _tutorial.SetStady(TutorialStady.FirstLevelPointer);
                }
                NextState?.Invoke(StateMashineStateType.Level);
                return;
            }

            if (_playerGameInfo.SettingsData.EnterInRewardScreen)
            {
                NextState?.Invoke(StateMashineStateType.Win);
            }
            else
            {
                NextState?.Invoke(StateMashineStateType.Upgrade);
                if (!_playerGameInfo.SettingsData.AdsDisabled)
                {
                    _adsSystem.ShowStartInterstitial(true);
                }
            }
        }

        public void Exit()
        {
            _gameLoader.Hide();
            _gameLoader.GameStarted -= GameLoaderOnGameStarted;
        }

        public event Action<StateMashineStateType> NextState;
    }
}