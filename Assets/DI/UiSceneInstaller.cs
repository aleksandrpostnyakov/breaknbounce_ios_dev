using Zenject;
using UnityEngine;
using UI;
using UI.Leaderboard;

public class UiSceneInstaller : MonoInstaller
{
    [SerializeField] private LeaderboardLine _leaderboardLinePrefab;   
    
    [SerializeField] private UiCanvasUserInput _touchUserInput;    
    [SerializeField] private UiCanvasUserInput _mouseUserInput;
    [SerializeField] private UpgradeScreen _upgradeScreen;
    [SerializeField] private ReviveScreen _reviveScreen;
    [SerializeField] private LoseScreen _loseScreen;
    [SerializeField] private WinScreen _winScreen;
    [SerializeField] private PauseScreen _pauseScreen;
    [SerializeField] private SettingsScreen _settingScreen;
    [SerializeField] private LeaderboardScreen _leaderboardScreen;
    [SerializeField] private HUD _hud;
    
    [Inject] private UiService _uiService;
    
    public override void InstallBindings()
    {
        if (Application.isMobilePlatform)
        {
            _mouseUserInput.gameObject.SetActive(false);
            Container.Bind<IUserInput>().FromInstance(_touchUserInput).AsSingle();
        }
        else
        {
            _touchUserInput.gameObject.SetActive(false);
            Container.Bind<IUserInput>().FromInstance(_mouseUserInput).AsSingle();
        }

        Container.Bind<UserInputWrapper>().AsSingle().NonLazy();
        Container.Bind<CameraHandler.CameraHandler>().AsSingle();
        
        Container.BindMemoryPool<LeaderboardLine, LeaderboardLine.Pool>().FromComponentInNewPrefab(_leaderboardLinePrefab);
        
        _uiService.Register(_upgradeScreen);
        _uiService.Register(_reviveScreen);
        _uiService.Register(_loseScreen);
        _uiService.Register(_winScreen);
        _uiService.Register(_pauseScreen);
        _uiService.Register(_settingScreen);
        _uiService.Register(_leaderboardScreen);
        _uiService.Register(_hud);
    }
}
