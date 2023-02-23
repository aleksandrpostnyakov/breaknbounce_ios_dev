using System;
using GameLoader;

namespace StateMashine
{
    public class LevelState : IState
    {
        private readonly GameLoader.GameLoader _gameLoader;
        public event Action<StateMashineStateType> NextState;

        public LevelState(GameLoader.GameLoader gameLoader)
        {
            _gameLoader = gameLoader;
            _gameLoader.LevelUnloaded += LevelUnloaded;
        }

        private void LevelUnloaded(LevelExitType levelExitType)
        {
            _gameLoader.LevelUnloaded -= LevelUnloaded;
            switch (levelExitType)
            {
                case LevelExitType.Win:
                    NextState?.Invoke(StateMashineStateType.Win);
                    break;
                case LevelExitType.WinTutorial:
                case LevelExitType.Close:
                    NextState?.Invoke(StateMashineStateType.Upgrade);
                    break;
                case LevelExitType.Lose:
                    NextState?.Invoke(StateMashineStateType.Lose);
                    break;
            }
        }

        public void Enter()
        {
            _gameLoader.LevelUnloaded += LevelUnloaded;
            _gameLoader.LoadLevel();
        }

        public void Exit()
        {
            
        }
    }
}