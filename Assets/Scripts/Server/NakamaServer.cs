using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using Nakama;
using Social;
using UnityEngine;
using Zenject;
using ISession = Nakama.ISession;

namespace Server
{
    public interface INakamaServer
    {
        Task<bool> Authentificate(bool direct = false);
        Task AuthenticateWithCustomId(string id);
        Task AuthenticateWithDevice();
        void AuthenticateWithFacebook();
        Task AuthenticateWithGmail();
        bool IsSession();
        void CloseSession();
        Client Client { get; }
        Task<IApiStorageObjects> ReadStorageObjectsAsync(Dictionary<string, string> datas);
        Task<IApiStorageObjectAcks> WriteStorageObjectsAsync(Tuple<string, string, string>[] datas);
        string GetUserName();
        void SetDeviceId(string deviceId);
        Task<bool> CheckSession(bool tryRestore = true);
        void TryReconnect();
        bool Connected { get;}

        public event Action<bool, string> Authenficated;
        event Action ReadStorageError;
        event Action WriteStorageError;
        event Action Reconnect;
    }

    public class NakamaServer:INakamaServer
    {
        [Inject(Id = "PlatformType")] PlatformType _platformType;
        
        private const string scheme = "https";
        private const string host = "mergeharvest.eu-west1-a.nakamacloud.io";
        private const int port = 443;
        private const string serverKey = "rJP6w4Q5nBBszvNH";

        private SocialService _socialService;

        private Client _client;
        private ISession _session;
        private string _deviceId;

        public bool Connected { get; private set; }

        public event Action<bool, string> Authenficated;
        
        public event Action ReadStorageError;
        public event Action WriteStorageError;
        public event Action Reconnect;

        public Client Client => _client;
        public ServerAuthenficationType AuthType;

        public NakamaServer(SocialService socialService)
        {
            _socialService = socialService;
        }
        
        public async Task<bool> Authentificate(bool direct = false)
        {
            if (direct)
            {
                Debug.Log($"ServerAuthentificate {direct}");
                return await InnerAuthentificate(true);
            }
            
            var serverResult = await CheckSession();
            if (!serverResult)
            {
                return await InnerAuthentificate();
            }
            else
            {
                Authenficated?.Invoke(true, string.Empty);
            }
            
            return true;
        }

        private async Task<bool> InnerAuthentificate(bool direct = false)
        {
#if !UNITY_EDITOR
            if (_platformType == PlatformType.VK)
            {
                _socialService.OnVKAuthenficated += SocialServiceOnOnAuthenficated;
                _socialService.VkNeedAuthenficate();
                return true;
            }
            else if (_platformType == PlatformType.OK)
            {
                _socialService.OnOKAuthenficated += SocialServiceOnOnAuthenficated;
                _socialService.OkNeedAuthenficate();
                return true;
            }
            else if (_platformType == PlatformType.Yandex && (AuthType == ServerAuthenficationType.Custom || AuthType == ServerAuthenficationType.None || direct))
            {
                Debug.Log("YANDEX InnerAuthentificate ");
                _socialService.OnYaAuthenficated += SocialServiceOnOnAuthenficated;
                _socialService.YaNeedAuthenficate(direct);
                return true;
            }
            else
#endif
            if (_platformType == PlatformType.Android ||_platformType == PlatformType.AndroidAds || _platformType == PlatformType.IOS || _platformType == PlatformType.GameDistribution || AuthType == ServerAuthenficationType.Device)
            {
                await  AuthenticateWithDevice();
                return true;
            }
            else
            {
                return false;
            }
        }
        
        
        private async void SocialServiceOnOnAuthenficated(string id)
        {
            _socialService.OnVKAuthenficated -= SocialServiceOnOnAuthenficated;
            _socialService.OnOKAuthenficated -= SocialServiceOnOnAuthenficated;
            _socialService.OnYaAuthenficated -= SocialServiceOnOnAuthenficated;
            await AuthenticateWithCustomId(id);
        }

        public void SetDeviceId(string deviceId)
        {
            _deviceId = deviceId;
        }

        private string _authTokenName => "nakama.authToken" + _platformType;
        private string _refreshTokenName => "nakama.refreshToken" + _platformType;

        public async void TryReconnect()
        {
            Debug.Log("TRY RECONNECT");
            Connected = false;
            var result = await CheckSession();
            if (result)
            {
                Reconnect?.Invoke();
            }
            else
            {
                if (AuthType == ServerAuthenficationType.None)
                {
                    WriteStorageError?.Invoke();
                }
                else
                {
                    await InnerAuthentificate();
                }
            }
        }

        private void OnReconnectAuth(bool result)
        {
            if (result)
            {
                Reconnect?.Invoke();
            }
            else
            {
                WriteStorageError?.Invoke();
            }
        }

        public async Task<bool> CheckSession(bool tryRestore = true)
        {
            if (_client == null)
            {
                var retryConfiguration = new RetryConfiguration(1000, 5, delegate
                {
                    //Debug.Log("NAKAMA SERVER TRY RECONNECT");
                });

                if (PlatformTypeHelper.IsWebGl(_platformType))
                {
                    _client = new Client(scheme, host, port, serverKey, UnityWebRequestAdapter.Instance);
                }
                else
                {
                    _client = new Client(scheme, host, port, serverKey);
                }

                _client.GlobalRetryConfiguration = retryConfiguration;
            }

            if (!tryRestore)
            {
                return false;
            }
            
            var authToken = PlayerPrefs.GetString(_authTokenName, null);
            var refreshToken = PlayerPrefs.GetString(_refreshTokenName, null);
            _session = Session.Restore(authToken, refreshToken);

            var authResult = false;
            if (_session == null)
            {
                return false;
            }
            
            if (_session.IsExpired || _session.HasExpired(DateTime.UtcNow.AddDays(1))) {
                try {
                    // Attempt to refresh the existing session.
                    _session = await _client.SessionRefreshAsync(_session);
                } catch (ApiResponseException e) {
                    Debug.Log("ApiResponseException " + e);
                    _session = null;
                    PlayerPrefs.SetString(_refreshTokenName, null);
                    return false;
                } catch (Exception e) {
                    Debug.Log("Exception " + e);
                    _session = null;
                    PlayerPrefs.SetString(_refreshTokenName, null);
                    return false;
                }
                
                PlayerPrefs.SetString(_authTokenName, _session.AuthToken);
            }

            Connected = true;
            return true;
        }

        private void SaveTokens()
        {
            PlayerPrefs.SetString(_refreshTokenName, _session.RefreshToken);
            PlayerPrefs.SetString(_authTokenName, _session.AuthToken);
        }
        
        private string GenerateUserName(string id)
        {
            const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var userName = string.Empty;
            foreach (var t in id)
            {
                userName += symbols[(100 +t) % symbols.Length];
            }

            return userName;
        }
        
        public async Task AuthenticateWithCustomId(string name)
        {
            var id = GenerateUserName(name + name);
            try
            {
                _session = await _client.AuthenticateCustomAsync(id, name);
                if (_session != null)
                {
                    SaveTokens();
                    Connected = true;
                    AuthType = ServerAuthenficationType.Custom;
                    Authenficated?.Invoke(true, "");
                    OnReconnectAuth(true);
                }
                
                Debug.Log("Authenticated with Custom ID");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error authenticating with Custom ID: {ex.Message}");
                Authenficated?.Invoke(false, ex.Message);
                OnReconnectAuth(false);
            }
        }

        public async Task AuthenticateWithDevice()
        {
            Debug.Log($"AuthenticateWithDevice {_deviceId}");
            try
            {
                _session = await _client.AuthenticateDeviceAsync(_deviceId, _deviceId);
                if (_session != null)
                {
                    SaveTokens();
                    Connected = true;
                    AuthType = ServerAuthenficationType.Device;
                    Authenficated?.Invoke(true, "");
                    OnReconnectAuth(true);
                }
                
                Debug.Log("Authenticated with Device ID");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error authenticating with Device ID: {ex.Message}");
                Authenficated?.Invoke(false, ex.Message);
                OnReconnectAuth(false);
            }
        }
        
        public async Task AuthenticateWithGmail()
        {
            const string playerIdToken = "...";
            try
            {
                var session = await _client.AuthenticateGoogleAsync(playerIdToken, _deviceId);
                if (_session != null)
                {
                    SaveTokens();
                    Connected = true;
                    AuthType = ServerAuthenficationType.Google;
                    Authenficated?.Invoke(true, "");
                    OnReconnectAuth(true);
                }
                
                Debug.Log("Authenticated with Device ID");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error authenticating with Device ID: {ex.Message}");
                Authenficated?.Invoke(false, ex.Message);
                OnReconnectAuth(false);
            }
        }
        
        public void AuthenticateWithFacebook()
        {
            // FB.LogInWithReadPermissions(new[] { "public_profile", "email" }, async result =>
            // {
            //     if (FB.IsLoggedIn)
            //     {
            //         try
            //         {
            //             _session = await _client.AuthenticateFacebookAsync(AccessToken.CurrentAccessToken.TokenString);
            //             if (_session != null)
            //             {
            //                 _analytics.SetUserName(_session.Username);
            //                 Bugsnag.SetUser(_deviceId, "user@example.com", _session.Username);
            //                 SaveTokens();
            //                 Connected = true;
            //                 AuthType = ServerAuthenficationType.Facebook;
            //                 Authenficated?.Invoke(true, string.Empty);
            //                 OnReconnectAuth(true);
            //             }
            //             Debug.Log("Authenticated with Facebook");
            //         }
            //         catch(Exception ex)
            //         {
            //             Debug.LogFormat("Error authenticating with Facebook: {0}", ex.Message);
            //             Authenficated?.Invoke(false, string.Empty);
            //             OnReconnectAuth(false);
            //         }
            //     }
            //     else
            //     {
            //         Debug.LogFormat("Error authenticating with Facebook: {0}", "NOLOGIN");
            //         Authenficated?.Invoke(false, string.Empty);
            //     }
            // });
        }

        public string GetUserName()
        {
            return _session == null ? "" : _session.Username;
        }
        
        public bool IsSession()
        {
            if (_session == null)
            {
                return false;
            }
            
            if (_session.IsExpired || _session.IsRefreshExpired)
            {
                return false;
            }

            return _session != null;
        }

        public async Task<IApiStorageObjects> ReadStorageObjectsAsync(Dictionary<string, string> datas)
        {
            var storageIds = new IApiReadStorageObjectId[datas.Count];
            var index = 0;
            foreach (var data in datas)
            {
                storageIds[index] = new StorageObjectId {Collection = data.Value, Key = data.Key, UserId = _session.UserId};
                index++;
            }
            try
            {
                var result = await Client.ReadStorageObjectsAsync(_session, storageIds);
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"NAKAMA ASYNC SERVER ERROR {e.Message}");
                Connected = false;
                ReadStorageError?.Invoke();
            }

            return null;
        }
        
        public async Task<IApiStorageObjectAcks> WriteStorageObjectsAsync(Tuple<string, string, string>[] datas)
        {
            var storageIds = new IApiWriteStorageObject[datas.Length];
            for (var i = 0; i < datas.Length; i++)
            {
                storageIds[i] = new WriteStorageObject
                {
                    Collection = datas[i].Item1,
                    Key = datas[i].Item2,
                    Value = datas[i].Item3,
                    PermissionRead = 1, // Only the server and owner can read
                    PermissionWrite = 1, // The server and owner can write
                };
                //Debug.Log($"Placed objects: [{string.Join(",  ", datas[i].Item2)}]");
            }
            
            try
            {
                var result = await Client.WriteStorageObjectsAsync(_session, storageIds);
                //Debug.Log($"Stored objects: [{string.Join(",\n  ", result)}]");
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"NAKAMA ASYNC SERVER ERROR {e.Message}");
                Connected = false;
                WriteStorageError?.Invoke();
                return null;
            }
        }
        
        public async void CloseSession()
        {
            if (_session != null)
            {
                try
                {
                    await _client.SessionLogoutAsync(_session);
                }
                catch (Exception e)
                {
                    Debug.LogError($"NAKAMA ASYNC SESION LOGOUT ERROR {e.Message}");
                }
                
            }
        }
    }

    public enum ServerAuthenficationType
    {
        None,
        Device,
        Custom,
        Facebook,
        Google,
    }
}