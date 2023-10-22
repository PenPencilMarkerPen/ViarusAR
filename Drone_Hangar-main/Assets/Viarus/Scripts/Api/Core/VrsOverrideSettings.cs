using UnityEngine;

namespace Vrs.Internal
{
    public static class VrsOverrideSettings
    {
        public enum PerfLevel
        {
            NoOverride = -1,
            System = 0,
            Minimum = 1,
            Medium = 2,
            Maximum = 3
        };

        public delegate void OnProfileChangedCallback();
        
        
        
        public static OnProfileChangedCallback OnProfileChangedEvent;

        
        public delegate void OnEyeCameraInitCallback(VrsViewer.Eye eye, GameObject goParent);
        
        
        
        public static OnEyeCameraInitCallback OnEyeCameraInitEvent;

        public delegate void OnGazeCallback(GameObject gazeObject);
        
        
        
        public static OnGazeCallback OnGazeEvent;
		
		public delegate void OnApplicationQuit();
        
        
        
        public static OnApplicationQuit OnApplicationQuitEvent;

    }
}
