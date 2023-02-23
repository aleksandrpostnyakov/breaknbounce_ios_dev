using System;
using PlayerInfo;
using TMPro;
using UI.Leaderboard;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class LeaderboardScreen : MonoBehaviour
    {
        [Inject] private PlayerGameInfo _playerInfo;
        [Inject] private CameraHandler.CameraHandler _cameraHandler;
        
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Button _closeBtn;
        [SerializeField] private TopMenu _topMenu;
        [SerializeField] private Leaderboard.Leaderboard _leaderboard;
        [SerializeField] private Transform _centralBlockTransform;
        [SerializeField] private Transform _uiTransform;

        public event Action OnExit;
        
        private void Awake()
        {
            var coef = _cameraHandler.GetAspect() / .5625f;
            float screenCoef = 1;
            if (coef != 1)
            {
                if (coef > 1)
                {
                    coef = 1 / coef;
                }

                var rt = _centralBlockTransform as RectTransform;
                var widthCoef = Screen.width / rt.rect.width;
                var heightCoef = Screen.height / rt.rect.height;

                if (widthCoef < 1 || heightCoef < 1)
                {
                    if (widthCoef < 1 && heightCoef < 1)
                    {
                        screenCoef = widthCoef < heightCoef ? widthCoef : heightCoef;
                    }
                    else if (widthCoef < 1)
                    {
                        screenCoef = widthCoef;
                    }
                    else
                    {
                        screenCoef = heightCoef;
                    }
                }
                else if (widthCoef > heightCoef)
                {
                    screenCoef = 1 / widthCoef;
                }
                else
                {
                    screenCoef = 1 / heightCoef;
                }
                
                var uiScale = _uiTransform.localScale.x;

                if (uiScale < 1)
                {
                    uiScale = 1 / uiScale;
                }

                screenCoef *= uiScale;
                _centralBlockTransform.localScale = new Vector3(screenCoef, screenCoef, 1);
            }
        }

        
        private void Start()
        {
            _closeBtn.onClick.AddListener(OnCloseClick);
        }
        
        public void Show()
        {
            _topMenu.Init();
            _leaderboard.Init(_playerInfo.GetXP, _playerInfo.GetOldXP, 5, true);
            
            _canvas.enabled = true;
        }
        
        public void Hide()
        {
            _canvas.enabled = false;
        }

        private void OnCloseClick()
        {
            OnExit?.Invoke();
        }
        
        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
        }
    }
}