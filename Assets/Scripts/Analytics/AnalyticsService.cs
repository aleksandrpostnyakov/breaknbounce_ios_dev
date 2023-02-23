using ByteBrewSDK;
using Mycom.Tracker.Unity;

namespace Analytics
{
    public class AnalyticsService
    {
        public AnalyticsService()
        {
            ByteBrew.InitializeByteBrew();
            InitializeMyTracker();
        }

        public void ByteBrewLevelEvent(int level, string value)
        {
                ByteBrew.NewCustomEvent("Level" , $"level={level};" + value);
                
        }

        public void ByteBrewLevelProgressionEvent(ByteBrewProgressionTypes type, int level, string value)
        {
                if (value != string.Empty)
                {
                        ByteBrew.NewProgressionEvent(type, "Level", $"level-{level}", value);
                }
                else
                {
                        ByteBrew.NewProgressionEvent(type, "Level", $"level-{level}");
                }
        }

        private void InitializeMyTracker()
        {
#if !UNITY_IOS && !UNITY_ANDROID || UNITY_EDITOR
        return;
#endif

        var myTrackerConfig = MyTracker.MyTrackerConfig;
        // ...
        // Настройте параметры трекера
        // ...

#if UNITY_IOS
        MyTracker.Init("SDK_KEY_IOS");
#elif UNITY_ANDROID
        MyTracker.Init("53273254060664435499");
#endif
        }
    }
}