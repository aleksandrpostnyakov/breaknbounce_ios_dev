
using System;
using System.Linq;
using UnityEngine;

public class UserInputWrapper: IDisposable
{
    private readonly IUserInput _userInput;
    private Camera _currentCamera;
    public bool CurrentIsMainCamera { get; private set; }
    
    public event Action<Transform> PointerUpInputReceived;
    public event Action<Transform> PointerDownInputReceived;
    public event Action<Vector2> ProcessedPointerUpInputReceived;
    public event Action<Vector2> ProcessedPointerDownInputReceived;
    public event Action<Vector2, Vector2> ProcessedPointerMoveInputReceived;
    public event Action<Transform[]> CustomCameraPointerUpInputReceived;
    public event Action<Transform[]> CustomCameraPointerDownInputReceived;
    public event Action<Vector2> CustomCameraProcessedPointerUpInputReceived;
    public event Action<Vector2> CustomCameraProcessedPointerDownInputReceived;

    public UserInputWrapper(IUserInput userInput)
    {
        _userInput = userInput;
        _userInput.PointerUpInputReceived += UserInput_PointerUpInputReceived;
        _userInput.PointerDownInputReceived += UserInput_PointerDownInputReceived;
        _userInput.InputMoveReceived += UserInput_InputMoveReceived;
    }

    private void UserInput_InputMoveReceived(Vector2 pos, Vector2 delta)
    {
        ProcessedPointerMoveInputReceived?.Invoke(pos, delta);
    }

    public void SetCurrentCamera(Camera camera, bool isMain)
    {
        _currentCamera = camera;
        CurrentIsMainCamera = isMain;
    }

    private void UserInput_PointerUpInputReceived(Vector2 pos)
    {
        if (_currentCamera == null)
        {
            return;
        }
        
        ProcessedPointerUpInputReceived?.Invoke(pos);
        return;
        
        var ray = _currentCamera.ScreenPointToRay(pos);
        var results = new RaycastHit[10];
        var raycasts = Physics.RaycastNonAlloc(ray, results);
        if (raycasts > 0)
        {
            results = results.Take(raycasts).OrderBy(r=>r.collider.gameObject.transform.position.y).ToArray();
            for (var i = 0; i < raycasts; i++)
            {
                //Debug.Log($"HIT {results[i].collider.gameObject.name} {results[i].collider.gameObject.transform.position.y}");
            }

            if (CurrentIsMainCamera)
            {
                PointerUpInputReceived?.Invoke(results[0].collider.gameObject.transform);
                ProcessedPointerUpInputReceived?.Invoke(new Vector2(100000, 100000));
            }
            else
            {
                CustomCameraPointerUpInputReceived?.Invoke(results.Select(c => c.collider.gameObject.transform).ToArray());
                CustomCameraProcessedPointerUpInputReceived?.Invoke(new Vector2(100000, 100000));
            }
        }
        else
        {
            if (CurrentIsMainCamera)
            {
                ProcessedPointerUpInputReceived?.Invoke(pos);
            }
            else
            {
                CustomCameraProcessedPointerUpInputReceived?.Invoke(pos);
            }
        }
    }
    
    private void UserInput_PointerDownInputReceived(Vector2 pos)
    {
        if (_currentCamera == null)
        {
            return;
        }
        
        ProcessedPointerDownInputReceived?.Invoke(pos);
        
        return;
        var ray = _currentCamera.ScreenPointToRay(pos);
        var results = new RaycastHit[10];
        var raycasts = Physics.RaycastNonAlloc(ray, results);
        if (raycasts > 0)
        {
            results = results.Take(raycasts).OrderBy(r=>r.collider.gameObject.transform.position.y).ToArray();
            for (var i = 0; i < raycasts; i++)
            {
               // Debug.Log($"HITDOWN {results[i].collider.gameObject.name} {results[i].collider.gameObject.transform.position.y}");
            }
            if (CurrentIsMainCamera)
            {
                PointerDownInputReceived?.Invoke(results[0].collider.gameObject.transform);
            }
            else
            {
                CustomCameraPointerDownInputReceived?.Invoke(results.Select(c => c.collider.gameObject.transform)
                    .ToArray());
            }
            
        }
        else
        {
            if (CurrentIsMainCamera)
            {
                ProcessedPointerDownInputReceived?.Invoke(pos);
            }
            else
            {
                CustomCameraProcessedPointerDownInputReceived?.Invoke(pos);
            }
        }
    }

    public void Dispose()
    {
        _userInput.PointerUpInputReceived -= UserInput_PointerUpInputReceived;
        _userInput.PointerDownInputReceived -= UserInput_PointerDownInputReceived;
    }
}

