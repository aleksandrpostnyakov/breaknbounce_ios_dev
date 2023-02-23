using UnityEngine;

namespace Funcraft.Merge
{
    public class LowMemoryTrigger :MonoBehaviour
    {
        private void Start()
        {
            Application.lowMemory += OnLowMemory;
        }
        
        private void OnLowMemory()
        {
            Debug.Log("On LOW MEMORY UNLOAD RESOURCES");
            Resources.UnloadUnusedAssets();
        }
    }
}