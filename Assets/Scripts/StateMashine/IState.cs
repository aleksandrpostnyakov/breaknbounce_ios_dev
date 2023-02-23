using System;

namespace StateMashine
{
    public interface IState
    {
        void Enter();
        void Exit();
        event Action<StateMashineStateType> NextState;
    }
}