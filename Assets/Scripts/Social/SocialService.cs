using System;
using UnityEngine;
using Zenject;

namespace Social
{
    public class SocialService
    {
        public event Action<int> OnSocialAction;
        public event Action OnInviteUsers;
        public event Action OnWallpostDataChanged;
        public event Action OnNeedVKAuthenficated;
        public event Action OnNeedOKAuthenficated;
        public event Action<bool> OnNeedYaAuthenficated;
        public event Action<string> OnVKAuthenficated;
        public event Action<string> OnOKAuthenficated;
        public event Action<string> OnYaAuthenficated;
        public event Action OnYaAuthenficatedError;
        
        public event Action<string> OnCopyToClipboard;
        
        public SocialWallpostData SocialWallpostData = new();
        
        public void SocialAction(int id)
        {
            OnSocialAction?.Invoke(id);
        }

        public void InviteUsers()
        {
            OnInviteUsers?.Invoke();
        }
        
        public void GridMerge()
        {
            SocialWallpostData.MergeCount++;
        }
        
        public void WallpostDataChanged()
        {
            OnWallpostDataChanged?.Invoke();
        }
        
        public void VkNeedAuthenficate()
        {
            OnNeedVKAuthenficated?.Invoke();
        }
        
        public void VkAuthenficated(string id)
        {
            Debug.Log("SOCIAL SERVICE ON AUTH " + id);
            OnVKAuthenficated?.Invoke(id);
        }
        
        public void OkNeedAuthenficate()
        {
            OnNeedOKAuthenficated?.Invoke();
        }
        
        public void OKAuthenficated(string id)
        {
            Debug.Log("SOCIAL SERVICE ON AUTH " + id);
            OnOKAuthenficated?.Invoke(id);
        }
        
        public void YaNeedAuthenficate(bool direct)
        {
            OnNeedYaAuthenficated?.Invoke(direct);
        }
        
        public void YaAuthenficated(string id)
        {
            Debug.Log("SOCIAL SERVICE ON AUTH " + id);
            OnYaAuthenficated?.Invoke(id);
        }
        
        public void YaAuthenficatedError()
        {
            Debug.Log("SOCIAL SERVICE ON AUTH ERROR");
            OnYaAuthenficatedError?.Invoke();
        }

        public void UpdateSocialWallpostData(SocialWallpostData data)
        {
            SocialWallpostData.Complete = SocialWallpostData.Complete || data.Complete;
            SocialWallpostData.TimeCount += data.TimeCount;
            SocialWallpostData.MergeCount += data.MergeCount;
        }

        public void CopyToClipboard(string text)
        {
            OnCopyToClipboard?.Invoke(text);
        }
    }
    
    [Serializable]
    public class SocialWallpostData
    {
        public int MergeCount;
        public int TimeCount;
        public bool Complete;
    }
}