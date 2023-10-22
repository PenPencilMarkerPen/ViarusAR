















using UnityEngine;
namespace Vrs.Internal
{
    public interface IVrsGazeResponder
    {
        
        
        void OnGazeEnter();

        
        
        void OnGazeExit();

        
        void OnGazeTrigger();

        
        
        
        
        
        void OnUpdateIntersectionPosition(Vector3 position);
    }
}