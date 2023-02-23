using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Reads user input from Ui image.
/// </summary>
[RequireComponent(typeof(Image))]

public abstract class UiCanvasUserInput : MonoBehaviour, IUserInput
{
    public event Action<Vector2> PointerDownInputReceived;
    public event Action<Vector2> PointerUpInputReceived;
    public event Action<Vector2, Vector2> InputMoveReceived;
    public event Action<float> InputScaleReceived;
    public event Action<float, float> InputRotateReceived;
    
    protected void InvokeScaleEvent(float scrollDelta)
    {
        InputScaleReceived?.Invoke(scrollDelta);

    }

    protected void InvokeMoveEvent(Vector2 position, Vector2 delta)
    {
        InputMoveReceived?.Invoke(position, delta);
    }

    protected void InvokeRotateEvent(Vector2 delta)
    {
        InputRotateReceived?.Invoke( Vector2.Dot(delta, Vector2.right), Vector2.Dot(delta, Vector2.up));
    }

    protected void InvokePointerDownEvent(Vector2 position)
    {
        PointerDownInputReceived?.Invoke(position);
    }
    protected void InvokePointerUpEvent(Vector2 position)
    {
        PointerUpInputReceived?.Invoke(position);
    }
    
}