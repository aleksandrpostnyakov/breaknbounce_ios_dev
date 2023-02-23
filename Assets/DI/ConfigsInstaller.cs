using Config;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "Data", menuName = "Config/ConfigsInstaller", order = 1)]
public class ConfigsInstaller : ScriptableObjectInstaller<ConfigsInstaller>
{
    [SerializeField] private AudioConfig _audioConfig;
    [SerializeField] private LevelsConfig _levelsConfig;
    [SerializeField] private GameConfig _gameConfig;
    [SerializeField] private EnemiesConfig _enemiesConfig;
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<AudioConfig>().FromInstance(_audioConfig).AsSingle();
        Container.Bind<LevelsConfig>().FromInstance(_levelsConfig).AsSingle();
        Container.Bind<GameConfig>().FromInstance(_gameConfig).AsSingle();
        Container.Bind<EnemiesConfig>().FromInstance(_enemiesConfig).AsSingle();
    }
}
