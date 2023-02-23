using Zenject;

namespace Core
{
    public enum PlatformType
    {
        Android,
        IOS,
        Yandex,
        VK,
        OK,
        GameDistribution,
        Facebook,
        AndroidAds
    }

    public class PlatformTypeHelper
    {
        public static bool IsWebGl(PlatformType type)
        {
            return type == PlatformType.Yandex || type == PlatformType.OK || type == PlatformType.VK || type == PlatformType.GameDistribution;
        }
        
        public static bool IsAdsPlatform(PlatformType type)
        {
            return IsWebGl(type) || type == PlatformType.AndroidAds;
        }
    }
}