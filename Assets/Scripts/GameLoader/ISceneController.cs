using System;
using GameLoader;

namespace Funcraft.Merge
{
    public interface ISceneController
    {
        public bool Initialized { get;}
        public event Action OnInitialized;
        public event Action<LevelExitType> OnExit;
        void Init();
    }
}