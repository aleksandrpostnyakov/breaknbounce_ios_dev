using System.Threading.Tasks;

namespace Save
{
    public interface ISaveSystem 
    {
        bool IsEmpty { get; }
        bool SoundSettingsSaved { get; }
        bool AdsSaveExists { get; }
        void LoadAdsFromSave();
        Task LoadFromSave();
        void Init();
        void DeInit();
        void LoadSoundsSettings();
        bool GameStarted { get;  set; }
        Task<bool> LoadFromServer();
        string GetUserId();
        string[] PreviousInstalledVersions { get; }
    }
}