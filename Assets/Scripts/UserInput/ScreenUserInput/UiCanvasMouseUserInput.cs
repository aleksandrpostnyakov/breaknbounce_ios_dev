
using UnityEngine.EventSystems;

public class UiCanvasMouseUserInput : UiCanvasUserInput, IScrollHandler, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                InvokeMoveEvent(eventData.position, eventData.delta);
                break;
            case PointerEventData.InputButton.Right:
                InvokeRotateEvent(eventData.delta);
                break;
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        InvokeScaleEvent(eventData.scrollDelta.y);
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        InvokePointerDownEvent(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InvokePointerUpEvent(eventData.position);
    }
}
