using Funcraft.Merge;
using Interhaptics.Platforms.Mobile;
using UI;
using UnityEngine;
using Zenject;

namespace DI
{
    public class LoadingSceneInstaller : MonoInstaller
    {
        [SerializeField] private LoadingScreen _loadingScreen;

        [Inject] private UiService _uiService;
        
        public override void InstallBindings()
        {
            _uiService.Register(_loadingScreen);
        }
    }
}