using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class UiCanvaseTouchUserInput : UiCanvasUserInput, IDragHandler,IPointerDownHandler, IPointerUpHandler
{
    private List<PointerData> _pointers = new ();
    private float _previousZoom;
    
    
    public void OnPointerDown(PointerEventData eventData)
    {
        var pointer = _pointers.FirstOrDefault(p => p.Id == eventData.pointerId);
        Debug.Log($"POINTER DOWN {eventData.pointerId}");
        if (pointer == null)
        {
            _pointers.Add(new PointerData(){Id = eventData.pointerId, Position = eventData.position});
            Debug.Log($"POINTER ADD {eventData.pointerId}");
        }
        else
        {
            pointer.Position = eventData.position;
        }
        
        if (_pointers.Count > 1)
        {
            _previousZoom = (_pointers[^1].Position - _pointers[^2].Position).magnitude;
          
            for (var i = 0; i < _pointers.Count-1; ++i)
            {
                var p = _pointers[i];
                InvokePointerUpEvent(p.Position);
            }

            //_pointers = _pointers.GetRange(_pointers.Count - 2, 2);
        }
        else if (_pointers.Count == 1)
        {
            InvokePointerDownEvent(eventData.position);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        //_pointers.RemoveAll(p => p.Id == eventData.pointerId);
        _pointers.Clear();
        Debug.Log($"POINTER UP {eventData.pointerId} {_pointers.Count}");
        if (_pointers.Count == 0)
        {
            InvokePointerUpEvent(eventData.position);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        var pointer = _pointers.FirstOrDefault(p => p.Id == eventData.pointerId);
        if (pointer != null)
        {
            pointer.Position = eventData.position;

            if (_pointers.Count > 1)
            {
                var zoom = (_pointers[0].Position - _pointers[1].Position).magnitude;
                if (_previousZoom > 0)
                {
                    InvokeScaleEvent((zoom - _previousZoom)/100);
                }
                _previousZoom = zoom;
            }
            else if (_pointers.Count > 0)
            {
                InvokeMoveEvent(eventData.position, eventData.delta/2);
            }
        }
    }
    
    private class PointerData
    {
        public int Id;
        public Vector2 Position;
    }
}