using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Nakama;
using Newtonsoft.Json;
using Server;
using Social;
using UnityEngine;
using Zenject;
using Nakama.TinyJson;
using Sound;

namespace Save
{
    public class CloudSave : ISaveSystem, IDisposable, ITickable
    {
        private readonly ISoundManager _soundManager;
        private readonly IAdsSystem _adsSystem;
        private readonly SocialService _socialService;
        private readonly INakamaServer _nakamaServer;
        private readonly IPurchaser _purchaser;
        
        private CancellationTokenSource _cts;
        private CloudSaveData _saveData;
        private List<string> _changed = new();
        private bool _inSave;
        private bool _isInitialized;
        private DateTime _lastSaveTime = DateTime.Now;
        private const float _savePause = 1f;
        private string _userId;
        
        private const string User_Name_Prefs = "User_Name_Prefs";
        
        [Inject(Id = "PlatformType")] PlatformType _platformType;

        public CloudSave( ISoundManager soundManager, SocialService socialService, IAdsSystem adsSystem,
                            INakamaServer nakamaServer, IPurchaser purchaser)
        {
            _adsSystem = adsSystem;
            _soundManager = soundManager;
            _socialService = socialService;
            _nakamaServer = nakamaServer;
            _purchaser = purchaser;

            Application.focusChanged += Application_focusChanged;
            
            _socialService.OnWallpostDataChanged += SocialServiceOnOnWallpostDataChanged;
            
            AuthenticateWithDevice();
        }

        private void AuthenticateWithDevice()
        {
            if (PlayerPrefs.HasKey(User_Name_Prefs))
            {
                _userId = PlayerPrefs.GetString(User_Name_Prefs);
            }
            else
            {
                var deviceId = SystemInfo.deviceUniqueIdentifier;
                if (deviceId == SystemInfo.unsupportedIdentifier)
                {
                    deviceId = Guid.NewGuid().ToString();
                }

                _userId = GenerateUserName(deviceId);
                PlayerPrefs.SetString(User_Name_Prefs, _userId);
            }
            
            var addCount = 10 - _userId.Length;
            for (var i = 0; i < addCount; i++)
            {
                _userId += "A";
            }
            
            _nakamaServer.SetDeviceId(_userId);
        }
        
        private string GenerateUserName(string deviceId)
        {
            Debug.Log($"GenerateUserName {deviceId}");
            const int blockLength = 4;
            const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var userName = string.Empty;
            for (var i = 0; i < deviceId.Length; i += blockLength)
            {
                var block = deviceId.Length - i > blockLength ? deviceId.Substring(i, blockLength) : deviceId.Substring(i, deviceId.Length - i);
                var sum = block.Aggregate(0, (current, t) => current + t);
                userName += symbols[sum % symbols.Length];
            }

            Debug.Log($"AfterGenerateUserName {userName}");
            return userName;
        }

        public bool IsEmpty => _saveData.Empty;
        public bool SoundSettingsSaved => _saveData.SoundSettings != null;
        public bool AdsSaveExists => _saveData.Ads != null;
        public void LoadAdsFromSave()
        {
            if (_saveData.Ads != null)
            {
                var json = _saveData.Ads;
                var model = JsonConvert.DeserializeObject<AdsModel>(json);
                _adsSystem.SetModel(model);
            }
        }

        public async Task LoadFromSave()
        {
        }
        
        private void AddToChanged(string id)
        {
            if (!_changed.Contains(id))
            {
                _changed.Add(id);
            }
        }

        private async Task SaveOnCloud(bool loadFromLocal)
        {
            _inSave = true;
            _lastSaveTime = DateTime.Now;
            var saveList = _changed.ToList();
            _changed.Clear();

            var list = saveList
                .Select(id => _saveData.GetSaveData(_platformType.ToString(), id))
                .Where(data => data != null)
                .ToArray();

            IApiStorageObjectAcks result = null;
            if (_nakamaServer.IsSession())
            {
                result = await _nakamaServer.WriteStorageObjectsAsync(list);
            }
            else
            {
                _nakamaServer.TryReconnect();
            }

            if (result == null)
            {
                foreach (var value in saveList)
                {
                    AddToChanged(value);
                }
            }
            
            _inSave = false;
        }

        private bool _directSave;
        
        public void Tick()
        {
            if ((DateTime.Now - _lastSaveTime).TotalSeconds < _savePause || _inSave)
            {
                return;
            }
            
            if (!_isInitialized || !_nakamaServer.Connected || (!Application.isFocused && !_directSave))
            {
                return;
            }
            
            if (_changed.Count > 0 && !_inSave)
            {
                SaveOnCloud(false);
                _directSave = false;
            }
        }

        public void Init()
        {
            _cts = new CancellationTokenSource();
            _isInitialized = true;
            _soundManager.Changed += SoundManager_Changed;
            _adsSystem.Changed += AdsSystem_Changed;

            SaveProcess();
        }
        
        private void SaveProcess()
        {
            Debug.Log("SaveProcess start");

        }

        public void LoadSoundsSettings()
        {
            var json = _saveData.SoundSettings;
            var model = JsonConvert.DeserializeObject<SoundManagerSettings>(json);
            _soundManager.Init(model);
        }

        public bool GameStarted { get; set; }
        
        public async Task<bool> LoadFromServer()
        {
            _saveData = new CloudSaveData();
            var dictionary = _saveData.GetSaveDataPair(_platformType.ToString());

            var datas = await _nakamaServer.ReadStorageObjectsAsync(dictionary);

            if (datas == null)
            {
                return false;
            }

            _saveData.ParseData(datas);
            return true;
        }
        
        public string GetUserId()
        {
            return _userId;
        }

        public string[] PreviousInstalledVersions => new string[0];
        
     

        #region +++ EVENTS +++
        private void SocialServiceOnOnWallpostDataChanged()
        {
            var json = JsonConvert.SerializeObject(_socialService.SocialWallpostData);
            _saveData.SocialWallPost = json;
            AddToChanged(CloudSaveData.SocialWallPoste_id);
        }
        
        private void Application_focusChanged(bool isFocused)
        {
            if (_saveData == null)
            {
                return;
            }

            if (isFocused)
            {
            }
        }

        
        private void SoundManager_Changed(bool needSave)
        {
            if (!needSave)
            {
                return;
            }
            var json = JsonConvert.SerializeObject(_soundManager.Settings);
            _saveData.SoundSettings = json;
            AddToChanged(CloudSaveData.SoundSettings_id);
        }
        
        private void AdsSystem_Changed()
        {
            var model = _adsSystem.Model;
            var json = JsonConvert.SerializeObject(model, new JsonSerializerSettings() {DateFormatHandling = DateFormatHandling.IsoDateFormat});
            _saveData.Ads = json;
            AddToChanged(CloudSaveData.Ads_id);
        }
        #endregion --- EVENTS ---
        
        public void Dispose()
        {
            Application.focusChanged -= Application_focusChanged;
            _socialService.OnWallpostDataChanged -= SocialServiceOnOnWallpostDataChanged;
            DeInit();
        }
        public void DeInit()
        {
            _cts?.Cancel();

            _isInitialized = false;
           
            _soundManager.Changed -= SoundManager_Changed;
            _adsSystem.Changed -= AdsSystem_Changed;
        }
    }

    public class CloudSaveData
    {
        public bool Empty = true;
        
        private int Flags;
        public string Ads;
        public string SoundSettings;
        public string SocialWallPost;
        public string UserData;
        public string SettingsData;
       
        public static readonly string Ads_id = "Ads";
        public static readonly string SoundSettings_id = "SoundSettings";
        public static readonly string SocialWallPoste_id = "SocialWallPost";
        public static readonly string UserData_id = "UserData";
        public static readonly string SettingsData_id = "SettingsData";
        
        public Dictionary<string, string> GetSaveDataPair(string collectionId)
        {
            return new()
            {
                {Ads_id, collectionId},
                {SoundSettings_id, collectionId},
                {SocialWallPoste_id, collectionId},
                {UserData_id, collectionId},
                {SettingsData_id, collectionId},
            };
        }
        
        public void ParseData(IApiStorageObjects data)
        {
            Empty = false;
            foreach (var dataObject in data.Objects)
            {
                switch (dataObject.Key)
                {
                    case "Flags":
                        Flags = int.Parse(GetStringFromJson(dataObject.Value));
                        break;
                    case "Ads":
                        Ads = dataObject.Value;
                        break;
                    case "SoundSettings":
                        SoundSettings = dataObject.Value;
                        break;
                    case "SocialWallPost":
                        SocialWallPost = dataObject.Value;
                        break;
                    case "UserData":
                        UserData = dataObject.Value;
                        break;
                    case "SettingsData":
                        SettingsData = dataObject.Value;
                        break;
                }
            }
        }

        public Tuple<string, string, string> GetSaveData(string collection, string key)
        {
            var data = key switch
            {
                "Flags" => ConvertStringToJson(Flags.ToString()),
                "Ads" => Ads,
                "SoundSettings" => SoundSettings,
                "SocialWallPost" => SocialWallPost,
                "UserData" => UserData,
                "SettingsData" => SettingsData,
                _ => null
            };
          
            return data != null ? new Tuple<string, string, string>(collection, key, data) : null;
        }

        public void SetFlag(CloudSaveDataFlags flagType, bool set = true)
        {
            var intFlag = (int) flagType;
            if ((Flags & intFlag) == intFlag)
            {
                if (!set)
                {
                    Flags -= intFlag;
                }
            }
            else
            {
                if (set)
                {
                    Flags += intFlag;
                }
            }
        }

        public bool IsFlag(CloudSaveDataFlags flagType)
        {
            var intFlag = (int) flagType;
            return (Flags & intFlag) == intFlag;
        }

        private string ConvertStringToJson(string value)
        {
            var data = new CloudSaveDataWrapper() {Value = value};
            return data.ToJson();
        }
        
        private string GetStringFromJson(string json)
        {
            var deserialized = json.FromJson<CloudSaveDataWrapper>();
            return deserialized.Value;
        }
    }

    public class CloudSaveDataWrapper
    {
        public string Value;
    }
    
    
    public enum CloudSaveDataFlags
    {
        Flag1 = 1,
    }
    
    
}