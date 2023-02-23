using Core;
using Server;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Funcraft.Merge
{
    public class LoginWindow : MonoBehaviour
    {
        [Inject] private INakamaServer _nakamaServer;
        
        [SerializeField] private Canvas _canvas;
        
        [SerializeField] private GameObject _yandexFirst;
        [SerializeField] private GameObject _yandexSecond;
        [SerializeField] private Button _yandexFirstYandexBtn;
        [SerializeField] private Button _yandexFirstGuestBtn;
        [SerializeField] private Button _yandexSecondYandexBtn;
        [SerializeField] private Button _yandexSecondGuestBtn;
        
        private bool _locked;

        private string _prefsYaGuestName = "YaPrefsGuest";

        private void Start()
        {
            _yandexFirstYandexBtn.onClick.AddListener(YandexLogin);
            _yandexFirstGuestBtn.onClick.AddListener(() =>
            {
                YandexGuestLogin(true);
            });
            _yandexSecondYandexBtn.onClick.AddListener(YandexLogin);
            _yandexSecondGuestBtn.onClick.AddListener(() =>
            {
                YandexGuestLogin(false);
            });
        }

        private void EnableGO(GameObject gameObject)
        {
            _yandexFirst.SetActive(gameObject == _yandexFirst);
            _yandexSecond.SetActive(gameObject == _yandexSecond);
        }

        private void ShowYandex()
        {
            EnableGO(_yandexFirst);
        }

        private async void YandexLogin()
        {
            Debug.Log("YandexLogin");
            if (_locked)
            {
                return;
            }

            await _nakamaServer.CheckSession(false);
            var result = await _nakamaServer.Authentificate(true);
            if (result)
            {
                PlayerPrefs.SetInt(_prefsYaGuestName, 0);
            }
            else
            {
                _locked = false;
            }
        }
        
        private void YandexGuestLogin(bool first)
        {
            if (_locked)
            {
                return;
            }
            
            if (first && PlayerPrefs.GetInt(_prefsYaGuestName, 0) != 1)
            {
                EnableGO(_yandexSecond);
            }
            else
            {
                SkipYaLogin();
            }
        }

        private async void SkipYaLogin()
        {
            if (_locked)
            {
                return;
            }

            PlayerPrefs.SetInt(_prefsYaGuestName, 1);
            _locked = true;
            await _nakamaServer.CheckSession(false);
            await _nakamaServer.AuthenticateWithDevice();
        }

        public bool IsGuestLogin(PlatformType _platformType)
        {
            if (_platformType == PlatformType.Yandex)
            {
                return PlayerPrefs.GetInt(_prefsYaGuestName, 0) == 1;
            }

            return false;
        }
        

        private void FacebookLogin()
        {
            if (_locked)
            {
                return;
            }

            _locked = true;
            _nakamaServer.AuthenticateWithFacebook();
        }
        
        private void GmailLogin()
        {
            if (_locked)
            {
                return;
            }

            _locked = true;
            _nakamaServer.AuthenticateWithGmail();
        }

        public void Show(PlatformType _platformType)
        {
            switch (_platformType)
            {
                case PlatformType.Yandex:
                default:
                    ShowYandex();
                    break;
            }
            _canvas.enabled = true;
        }

        public void Hide()
        {
            _locked = false;
            _canvas.enabled = false;
        }

        public void Error(string message)
        {
            _locked = false;
            Debug.Log(message);
        }

        private void OnDestroy()
        {
            _yandexFirstYandexBtn.onClick.RemoveAllListeners();
            _yandexFirstGuestBtn.onClick.RemoveAllListeners();
            _yandexSecondYandexBtn.onClick.RemoveAllListeners();
            _yandexSecondGuestBtn.onClick.RemoveAllListeners();
        }
    }
}