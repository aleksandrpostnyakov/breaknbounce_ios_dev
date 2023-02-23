
using System;
using System.Threading.Tasks;
using Core;
using Funcraft.Merge;
using Save;
using Server;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace GameLoader
{
    public class GameLoader : ITickable, IDisposable
    {
        [Inject] private UiService _uiService;
        [Inject] private ISaveSystem _saveSystem;
        [Inject] private IAdsSystem _adsSystem;
        [Inject] private INakamaServer _nakamaServer;
        [Inject(Id = "PlatformType")] PlatformType _platformType;

        private AsyncOperation _loadingSceneOperation;
        public event Action GameStarted;
        public event Action<LevelExitType> LevelUnloaded;

        private string _userId;
        private ISceneController _initial;

        private const string UISceneName = "UIScene";
        private const string LevelSceneName = "LevelScene";
        private LoadingScreen _loadingScreen;

        public async void Initialize()
        {
            _loadingScreen = _uiService.Get<LoadingScreen>();
            if (_loadingScreen == null)
            {
                Debug.LogError("ERROR GAME LOADER CANT LOAD LOADING SCREEN");
                return;
            }

            _loadingScreen.Initialize();
            _loadingScreen.OnButtonPlayClicked += ButtonPlayClicked;

            await new WaitForEndOfFrame();

            LoadUIScene();
        }

        private void LoadUIScene()
        {
            _loadingSceneOperation = SceneManager.LoadSceneAsync(UISceneName, LoadSceneMode.Additive);
            _loadingSceneOperation.completed += UiSceneLoaded;
        }

        private async void UiSceneLoaded(AsyncOperation asyncOperation)
        {
            _loadingSceneOperation.completed -= UiSceneLoaded;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(UISceneName));
            //var rootGOObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            //Authentificate();
            await LoadSave();
            //AfterSavePrepared();
        }

        private async void Authentificate()
        {
            _nakamaServer.Authenficated += OnAuthenficated;
            await _nakamaServer.CheckSession(false);

            var result = await _nakamaServer.Authentificate(_platformType != PlatformType.Yandex);
            if (!result)
            {
                _loadingScreen.ShowLoginWindow();
            }
        }

        private async void OnAuthenficated(bool result, string message)
        {
            if (result)
            {
                _nakamaServer.Authenficated -= OnAuthenficated;

                _userId = _nakamaServer.GetUserName();
                _loadingScreen.OnAuthenficated(_userId);

                await LoadSave();
            }
            else
            {
                _loadingScreen.ShowError($"ERROR {message}");
            }
        }

        private async Task LoadSave()
        {
            //_nakamaServer.ReadStorageError += NakamaServerOnReadStorageError;
            var result = await _saveSystem.LoadFromServer();
            if (result)
            {
               // _nakamaServer.ReadStorageError -= NakamaServerOnReadStorageError;
                AfterSavePrepared();
                _saveSystem.Init();
            }
        }

        private void NakamaServerOnReadStorageError()
        {
            _nakamaServer.Reconnect -= NakamaServerOnReconnect;
            _nakamaServer.Reconnect += NakamaServerOnReconnect;
        }

        private async void NakamaServerOnReconnect()
        {
            _nakamaServer.Reconnect -= NakamaServerOnReconnect;
            await LoadSave();
        }

        private void AfterSavePrepared()
        {
            //_loadingScreen.ShowPlayButton();
            ButtonPlayClicked();

            if (_platformType == PlatformType.OK || _platformType == PlatformType.GameDistribution)
            {
                _adsSystem.LoadAds();
            }
        }

        public void Hide()
        {
            _loadingScreen.Hide();
        }

        public void LoadLevel()
        {
            _loadingSceneOperation = SceneManager.LoadSceneAsync(LevelSceneName, LoadSceneMode.Additive);
            _loadingSceneOperation.completed += LoadingSceneOperation_completed;
        }

        private void LoadingSceneOperation_completed(AsyncOperation asyncOperation)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(LevelSceneName));
            _loadingSceneOperation.completed -= LoadingSceneOperation_completed;
            var rootGOObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var go in rootGOObjects)
            {
                _initial = go.GetComponentInChildren<ISceneController>();
                if (_initial != null)
                {
                    if (_initial.Initialized)
                    {
                        OnLevelSceneInit();
                    }
                    else
                    {
                        _initial.OnInitialized += OnLevelSceneInit;
                    }

                    break;
                }
            }
        }

        private void OnLevelSceneInit()
        {
            _initial.OnInitialized -= OnLevelSceneInit;
            _initial.OnExit += OnLevelSceneExit;
            _initial.Init();
        }

        private LevelExitType _levelExitType;

        private void OnLevelSceneExit(LevelExitType levelExitType)
        {
            _levelExitType = levelExitType;
            _initial.OnExit -= OnLevelSceneExit;
            _loadingSceneOperation = SceneManager.UnloadSceneAsync(LevelSceneName);
            _loadingSceneOperation.completed += UnLoadingSceneOperation_completed;
        }

        private void UnLoadingSceneOperation_completed(AsyncOperation asyncOperation)
        {
            _loadingSceneOperation.completed -= LoadingSceneOperation_completed;
            LevelUnloaded?.Invoke(_levelExitType);
        }

        private void ButtonPlayClicked()
        {
            _saveSystem.GameStarted = true;
            GameStarted?.Invoke();
        }

        public void Tick()
        {
            if (_loadingSceneOperation != null)
            {
                _loadingScreen.UpdateProgress(_loadingSceneOperation.progress);
            }
        }

        public void Dispose()
        {
            if (_loadingScreen != null)
            {
                _loadingScreen.OnButtonPlayClicked -= ButtonPlayClicked;
            }
        }
    }

    public enum LevelExitType
    {
        Win,
        Lose,
        Close,
        WinTutorial
    }
}

