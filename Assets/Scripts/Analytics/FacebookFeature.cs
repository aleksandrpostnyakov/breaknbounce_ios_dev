using Core;
using Facebook.Unity;
using UnityEngine;
using Zenject;

namespace Analytics
{
    public class FacebookFeature : MonoBehaviour
    {
        [Inject(Id = "PlatformType")] PlatformType _platformType;
        
        private void Start()
        {
            Init();
        }

        private void Init()
        {
            if (_platformType == PlatformType.Facebook)
            {
                // INIT IN WEBGL FACEBOOK SDK
                return;
            }
            
            FB.Init(InitComplete, HideUnity);
            Debug.Log("FacebookManager: FB.Init() called with " + FB.AppId);
        }

        private void InitComplete()
        {
            if (FB.IsInitialized) {
                // Signal an app activation App Event
                FB.ActivateApp();
                // Continue with Facebook SDK
                // ...
            } else {
                Debug.Log("Failed to Initialize the Facebook SDK");
            }
            return;
            var logMessage =
                $"FacebookManager: InitCompleteCalled: IsLoggedIn='{FB.IsLoggedIn}' IsInitialized='{FB.IsInitialized}'";
            Debug.Log(logMessage);
            if (AccessToken.CurrentAccessToken != null)
                Debug.Log(AccessToken.CurrentAccessToken.ToString());
        }

        private void HideUnity(bool isGameShown)
        {
            string.Format("Success Response: HideUnity Called {0}\n", isGameShown);
            Debug.Log("FacebookManager: Is game shown: " + isGameShown);
        }
    }
}