using System;
using Core;
using Social;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Funcraft.Merge
{
    public class LoadingScreen : MonoBehaviour
    {
        [Inject(Id = "PlatformType")] PlatformType _platformType;
        [Inject] private SocialService _socialService;
        
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Button _buttonPlay;
        [SerializeField] private Button _buttonCopy;
        [SerializeField] private Button _buttonInvite;
        [SerializeField] private TMP_Text _userNameText;
        [SerializeField] private LoginWindow _loginWindow;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private GameObject _eventSystem;
        [SerializeField] private GameObject _rus;
        [SerializeField] private GameObject _en;

        public event Action OnButtonPlayClicked;

        public void Initialize()
        {
            var directRussian = false;
            if (_platformType != PlatformType.VK)
            {
                _buttonInvite.gameObject.SetActive(false);
            }
            else
            {
                _buttonInvite.onClick.AddListener(() =>
                {
                    _socialService.InviteUsers();
                });
            
                I2.Loc.LocalizationManager.CurrentLanguage = "Russian";
                directRussian = true; 
            }
            
            _rus.SetActive(Application.systemLanguage == SystemLanguage.Russian || directRussian);
            _en.SetActive(Application.systemLanguage != SystemLanguage.Russian && !directRussian);
            
            _buttonPlay.gameObject.SetActive(false);
            _buttonPlay.onClick.AddListener(ButtonPlayClicked);
            _buttonCopy.onClick.AddListener(ButtonCopyClicked);
            _socialService.OnYaAuthenficatedError += ShowLoginWindow;
        }
        
        public void ShowLoginWindow()
        {
            _progressBar.gameObject.SetActive(false);
            _loginWindow.Show(_platformType);
        }

        public void OnAuthenficated(string userId)
        {
            _loginWindow.Hide();
            _userNameText.text = userId;
            _progressBar.gameObject.SetActive(true);
        }

        public void ShowError(string error)
        {
            _loginWindow.Error(error);
        }

        public void UpdateProgress(float progress)
        {
            _progressBar.value = progress;
        }
        
        public void ShowPlayButton()
        {
            _progressBar.gameObject.SetActive(false);
            if (_platformType == PlatformType.Yandex && _loginWindow.IsGuestLogin(_platformType))
            {
                ButtonPlayClicked();
            }
            else
            {
                _buttonPlay.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void ButtonPlayClicked()
        {
            OnButtonPlayClicked?.Invoke();
        }
        
        private void ButtonCopyClicked()
        {
            var _userId = _userNameText.text;
            GUIUtility.systemCopyBuffer = _userId;
            _socialService.CopyToClipboard(_userId);
            Debug.Log($"User name {_userId} copied");
        }

        private void OnDestroy()
        {
            _buttonInvite.onClick.RemoveAllListeners();
            _buttonPlay.onClick.RemoveAllListeners();
            _buttonCopy.onClick.RemoveAllListeners();
            _socialService.OnYaAuthenficatedError -= ShowLoginWindow;
        }
    }
}