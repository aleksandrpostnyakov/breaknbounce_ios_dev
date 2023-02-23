using System;
using Config;
using PlayerInfo;
using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class LoseScreen : MonoBehaviour
    {
        [Inject] private CameraHandler.CameraHandler _cameraHandler;
        [Inject] private ISoundManager _soundManager;
        
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Button _playBtn;
        [SerializeField] private Button _homeBtn;
        [SerializeField] private TopMenu _topMenu;
        [SerializeField] private UpgradesPanel _upgradesPanel;
        [SerializeField] private Transform _centralBlockTransform;
        [SerializeField] private Transform _uiTransform;
        
        
        [SerializeField] private Transform _upgradesTransform;

        public event Action OnPlay;
        public event Action OnHome;
        
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
                
                _homeBtn.transform.localScale = new Vector3(coef, coef, 0);
            }
        }

        private void Start()
        {
            _playBtn.onClick.AddListener(OnPlayClick);
            _homeBtn.onClick.AddListener(OnHomeClick);
        }

        public void Show()
        {
            // var coef = Mathf.Min(_cameraHandler.GetAspect() / .5625f, 1);
            // _upgradesTransform.localScale = new Vector3(coef, coef, 1);
            // var left = (coef - 1) * 450;
            // var rt = (RectTransform) _upgradesTransform;
            // rt.offsetMin = new Vector2(left, rt.offsetMin.y);
            // LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            _upgradesPanel.Init();
            _topMenu.Init();
            _soundManager.PlayUISound(UISoundId.Lose);
            _soundManager.PlayBackgroundMusic(BackgroundSoundId.MusicMainMenu);
            
            _canvas.enabled = true;
        }
        
        public void Hide()
        {
            _canvas.enabled = false;
        }
        
        private void OnPlayClick()
        {
            OnPlay?.Invoke();
        }
        
        private void OnHomeClick()
        {
            OnHome?.Invoke();
        }

        private void OnDestroy()
        {
            _playBtn.onClick.RemoveAllListeners();
            _homeBtn.onClick.RemoveAllListeners();
        }
    }
}