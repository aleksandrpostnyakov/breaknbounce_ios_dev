using System;
using Unity.Services.Authentication;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;

namespace Analytics
{
    public interface IUnityServicer
    {
        T GetConfig<T>(UnityServicer.RemoteConfigItemId itemId, T defaultValue);
        bool IsInitialized();

        event Action Initalized;
    }
    
    public class UnityServicer : IUnityServicer
    {
        private bool _isInitialized;
        public event Action Initalized;

        public bool IsInitialized()
        {
            return _isInitialized;
        }

        public UnityServicer()
        {
#if UNITY_WEBGL
            InitializationComplete();
#else
            Init();
#endif
        }
        
        private async void Init()
        {
            if (Utilities.CheckForInternetConnection())
            {
                var options = new InitializationOptions();
                //options.SetAnalyticsUserId("some-user-id");

                await UnityServices.InitializeAsync(options);
                //List<string> consentIdentifiers = await AnalyticsService.Instance.CheckForRequiredConsents();
                
                SingInRemoteControl();
            }
            else
            {
                Debug.Log("CAN NOT CONNECT TO UNITY SERVICES");
                InitializationComplete();
            }
        }

        private void InitializationComplete()
        {
            _isInitialized = true;
            Initalized?.Invoke();
        }

        #region ++++++ REMOTE CONFIG +++++
        private RuntimeConfig _appConfig;
        public enum RemoteConfigItemId
        {
            PLAYERSTATS_XP,
            PLAYERSTATS_COINS,
            PLAYERSTATS_GEMS,
            PLAYERSTATS_BOXES,
            PLAYERSTATS_ENERGY,
        }

        private async void SingInRemoteControl()
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }
                catch (Exception e)
                {
                    Debug.Log($"UNITY SERVICE SING IN ERROR : {e.Message}" );
                    InitializationComplete();
                }
            }
            
            RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
            RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
        }
        
        private struct userAttributes {
        }

        private struct appAttributes {
        }

        private void ApplyRemoteSettings (ConfigResponse configResponse) {
            switch (configResponse.requestOrigin) {
                case ConfigOrigin.Default:
                    Debug.Log ("No settings loaded this session; using default values.");
                    break;
                case ConfigOrigin.Cached:
                    //Debug.Log ("No settings loaded this session; using cached values from a previous session.");
                    //break;
                case ConfigOrigin.Remote:
                    Debug.Log ("New settings loaded this session; update values accordingly.");
                    _appConfig = RemoteConfigService.Instance.appConfig;
                    break;
            }

            InitializationComplete();
        }

        public T GetConfig<T>(RemoteConfigItemId itemId, T defaultValue)
        {
            T result;
            object value = null;
            
            if (_appConfig == null)
            {
                return defaultValue;
            }

            var typeOfT = typeof(T);
            if (typeOfT == typeof(int))
            {
                var defInt = (int)Convert.ChangeType(defaultValue, typeof(int));
                value = itemId switch
                {
                    RemoteConfigItemId.PLAYERSTATS_XP => _appConfig.GetInt("PlayerStats_InitialXP", defInt), 
                    RemoteConfigItemId.PLAYERSTATS_COINS => _appConfig.GetInt("PlayerStats_InitialCoins", defInt),
                    RemoteConfigItemId.PLAYERSTATS_GEMS => _appConfig.GetInt("PlayerStats_InitialGems", defInt),
                    RemoteConfigItemId.PLAYERSTATS_BOXES => _appConfig.GetInt("PlayerStats_InitialBoxes", defInt),
                    _ => null
                };
            }
            else if (typeOfT == typeof(float))
            {
                var defFloat = (float)Convert.ChangeType(defaultValue, typeof(float));
                value = itemId switch
                {
                    RemoteConfigItemId.PLAYERSTATS_ENERGY => _appConfig.GetFloat("PlayerStats_InitialEnergy", defFloat),
                    _ => null
                };
            }
            
            if (value == null)
            {
                return defaultValue;
            }

            try
            {
                result = (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception e)
            {
                Debug.Log("ERROR CONVERT " + e.Message);
                return defaultValue;
            }

            return result;
        } 
        #endregion --- REMOTE CONFIG -----
    }
}