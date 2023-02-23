using Effects;
using Interhaptics.Platforms.Mobile;
using Level;
using Zenject;
using UnityEngine;
using UI;

public class LevelSceneInstaller : MonoInstaller
{
    [SerializeField] private Field _fieldInstance;
    [SerializeField] private MobileHapticsVibration _hapticsVibration;
    public override void InstallBindings()
    {
        Container.Bind<BrickPools>().AsSingle();
        Container.Bind<Field>().FromInstance(_fieldInstance).AsSingle();
        Container.Bind<BricksMover>().AsSingle();
        Container.Bind<AttackWarning>().AsSingle();
        Container.Bind<MobileHapticsVibration>().FromInstance(_hapticsVibration).AsSingle();
        Container.Bind<Effector>().AsSingle();
    }
}