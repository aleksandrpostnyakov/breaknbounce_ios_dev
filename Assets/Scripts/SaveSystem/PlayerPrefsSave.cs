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
using PlayerInfo;
using Sound;

namespace Save
{
    public class PlayerPrefsSave : ISaveSystem, IDisposable, ITickable
    {
        private readonly ISoundManager _soundManager;
        private readonly IAdsSystem _adsSystem;
        private readonly SocialService _socialService;
        private readonly IPurchaser _purchaser;
        private readonly PlayerGameInfo _playerGameInfo;
        
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

        public PlayerPrefsSave( ISoundManager soundManager, SocialService socialService, IAdsSystem adsSystem,
                             IPurchaser purchaser, PlayerGameInfo playerGameInfo)
        {
            _adsSystem = adsSystem;
            _soundManager = soundManager;
            _socialService = socialService;
            _purchaser = purchaser;
            _playerGameInfo = playerGameInfo;

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
            
            //_nakamaServer.SetDeviceId(_userId);
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

            foreach (var saveId in saveList)
            {
                switch (saveId)
                {
                    case "Ads":
                        PlayerPrefs.SetString(saveId, _saveData.Ads);
                        break;
                    case "SoundSettings":
                        PlayerPrefs.SetString(saveId, _saveData.SoundSettings);
                        break;
                    case "SocialWallPost":
                        PlayerPrefs.SetString(saveId, _saveData.SocialWallPost);
                        break;
                    case "UserData":
                        PlayerPrefs.SetString(saveId, _saveData.UserData);
                        break;
                    case "SettingsData":
                        PlayerPrefs.SetString(saveId, _saveData.SettingsData);
                        break;
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
            
            if (!_isInitialized || (!Application.isFocused && !_directSave))
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
            _playerGameInfo.Changed += UserInfoChanged;
            _playerGameInfo.SettingsChanged += UserSettingsChanged;

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

            foreach (var saveId in dictionary)
            {
                if (PlayerPrefs.HasKey(saveId.Key))
                {
                    _saveData.Empty = false;
                    var json = PlayerPrefs.GetString(saveId.Key);
                    switch (saveId.Key)
                    {
                        case "Ads":
                            var amodel = JsonConvert.DeserializeObject<AdsModel>(json);
                            _saveData.Ads = json;
                            _adsSystem.SetModel(amodel);
                            break;
                        case "SoundSettings":
                            var obj = JsonConvert.DeserializeObject<SoundManagerSettings>(json);
                            _saveData.SoundSettings = json;
                            _soundManager.Init(obj);
                            break;
                        case "SocialWallPost":
                            var data = JsonConvert.DeserializeObject<SocialWallpostData>(json);
                            _saveData.SocialWallPost = json;
                            _socialService.UpdateSocialWallpostData(data);
                            break;
                        case "UserData":
                            var userData = JsonConvert.DeserializeObject<PlayerData>(json);
                            _saveData.UserData = json;
                            _playerGameInfo.Initialize(userData);
                            break;
                        case "SettingsData":
                            var settingsData = JsonConvert.DeserializeObject<SettingsData>(json);
                            _saveData.SettingsData = json;
                            _playerGameInfo.InitializeSettings(settingsData);
                            break;
                    }
                }
            }

            if (_saveData.Empty)
            {
                _soundManager.Init(null);
                _playerGameInfo.Initialize();
            }

            if (_playerGameInfo.SettingsData == null)
            {
                _playerGameInfo.InitializeSettings(null);
            }
            
            if (_soundManager.Settings == null)
            {
                _soundManager.Init(null);
            }

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

        private void UserInfoChanged(bool needSave)
        {
            if (!needSave)
            {
                return;
            }
            var json = JsonConvert.SerializeObject(_playerGameInfo.Data);
            _saveData.UserData = json;
            AddToChanged(CloudSaveData.UserData_id);
        }
        
        private void UserSettingsChanged(bool needSave)
        {
            if (!needSave)
            {
                return;
            }
            var json = JsonConvert.SerializeObject(_playerGameInfo.SettingsData);
            _saveData.SettingsData = json;
            AddToChanged(CloudSaveData.SettingsData_id);
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
            _playerGameInfo.Changed -= UserInfoChanged;
            _playerGameInfo.SettingsChanged -= UserSettingsChanged;
        }
    }
  }