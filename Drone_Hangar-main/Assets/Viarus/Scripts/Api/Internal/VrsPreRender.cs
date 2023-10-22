













using UnityEngine;








namespace Vrs.Internal
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("VRS/Internal/VrsPreRender")]
    public class VrsPreRender : MonoBehaviour
    {

        public Camera cam { get; private set; }

        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void Reset()
        {
#if UNITY_EDITOR
            
            
            var cam = GetComponent<Camera>();
#endif
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 0;
            cam.useOcclusionCulling = false;
            cam.depth = -100;
            cam.farClipPlane = 100;
            if (VrsGlobal.isVR9Platform)
            {
                cam.clearFlags = CameraClearFlags.Nothing;
            }
        }
    }
}