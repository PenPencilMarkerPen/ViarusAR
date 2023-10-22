













using UnityEngine;

















namespace Vrs.Internal
{
    public interface IVrsGazePointer
    {
        
        void OnGazeEnabled();
        
        void OnGazeDisabled();

        
        
        
        
        
        
        void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                         bool isInteractive);

        
        
        
        
        
        
        void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                        bool isInteractive);

        
        
        
        
        
        
        
        void OnGazeExit(Camera camera, GameObject targetObject);

        
        
        void OnGazeTriggerStart(Camera camera);

        
        
        void OnGazeTriggerEnd(Camera camera);

        
        
        
        
        
        
        void GetPointerRadius(out float innerRadius, out float outerRadius);

        void UpdateStatus();
    }
}