using System;
using UnityEngine;

public interface IUserInput 
{
    event Action<Vector2> PointerDownInputReceived;
    event Action<Vector2> PointerUpInputReceived;
    event Action<Vector2, Vector2> InputMoveReceived;
    event Action<float> InputScaleReceived;
    event Action<float, float> InputRotateReceived;

}
