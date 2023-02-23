using System;

namespace Level
{
    public class Tutorial
    {
        private TutorialStady _tutorialStady;

        public Action<int> Changed;

        public TutorialStady GetStady()
        {
            return _tutorialStady;
        }

        public void SetStady(TutorialStady stady)
        {
            _tutorialStady = stady;
            Changed?.Invoke((int) _tutorialStady);
        }
        
        public void SetStady(int stady)
        {
            _tutorialStady = ((TutorialStady)stady);
        }

        public bool IsFinished()
        {
            return _tutorialStady == TutorialStady.Finish;
        }
    }

    public enum TutorialStady
    {
        Start = 0,
        FirstLevelPointer = 1,
        FirstLevelPointerUp = 2,
        WinFirstLevel = 3,
        Finish = 4
    }
}