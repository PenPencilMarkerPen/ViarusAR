













#if VIARUS_VR
#define VRS_HACK
#endif

using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.Rendering;

namespace Vrs.Internal
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("VRS/Controller/VrsStereoController")]
    public class VrsStereoController : MonoBehaviour
    {
        
        private bool renderedStereo = false;

        
        private VrsEye[] eyes;
        private VrsHead mHead;

        
        
        
        
        public VrsEye[] Eyes
        {
            get
            {
                if (eyes == null)
                {
                    eyes = GetComponentsInChildren<VrsEye>(true)
                           .Where(eye => eye.Controller == this)
                           .ToArray();
                }

                if (eyes == null)
                {
                    VrsEye[] VrsEyess = FindObjectsOfType<VrsEye>();
                    if (VrsEyess.Length > 0)
                    {
                        eyes = VrsEyess;
                    }
                }
                return eyes;
            }
        }

        
        
        public VrsHead Head
        {
            get
            {
#if UNITY_EDITOR
                VrsHead mHead = null;  
#endif
                if (mHead == null)
                {
                    mHead = FindObjectOfType<VrsHead>();
                }
                return mHead;
            }
        }

        public Camera cam { get; private set; }

        void Awake()
        {
            VrsViewer.Create();
            cam = GetComponent<Camera>();
            AddStereoRig();

            VrsOverrideSettings.OnProfileChangedEvent += OnProfileChanged;
        }

        void OnProfileChanged()
        {
            Debug.Log("OnProfileChanged");
            VrsEye[] eyes = VrsViewer.Instance.eyes;
            foreach (VrsEye eye in eyes)
            {
                if (eye != null)
                {
                    eye.UpdateCameraProjection();
                }
            }
        }

        
        
        public void AddStereoRig()
        {
            Debug.Log("AddStereoRig.CreateEye");
            CreateEye(VrsViewer.Eye.Left);
            CreateEye(VrsViewer.Eye.Right);

            if (Head == null)
            {
               gameObject.AddComponent<VrsHead>();
               
               
            }
            Head.SetTrackPosition(VrsViewer.Instance.TrackerPosition);
        }

        
        private void CreateEye(VrsViewer.Eye eye)
        {
            string nm = name + (eye == VrsViewer.Eye.Left ? " Left" : " Right");
            VrsEye[] eyes = GetComponentsInChildren<VrsEye>();
            VrsEye mVrsEye = null;
            if (eyes != null && eyes.Length > 0)
            {
                foreach(VrsEye mEye in eyes)
                {
                    if(mEye.eye == eye)
                    {
                        mVrsEye = mEye;
                        break;
                    }
                }
            }
            
            if (mVrsEye == null)
            {
                GameObject go = new GameObject(nm);
                go.transform.SetParent(transform, false);
                go.AddComponent<Camera>().enabled = false;
                mVrsEye = go.AddComponent<VrsEye>();
            }

            if(VrsOverrideSettings.OnEyeCameraInitEvent != null) VrsOverrideSettings.OnEyeCameraInitEvent(eye, mVrsEye.gameObject);

            mVrsEye.Controller = this;
            mVrsEye.eye = eye;
            mVrsEye.CopyCameraAndMakeSideBySide(this);
            mVrsEye.OnPostRenderListener += OnPostRenderListener;
            mVrsEye.OnPreRenderListener += OnPreRenderListener;
            VrsViewer.Instance.eyes[eye == VrsViewer.Eye.Left ? 0 : 1] = mVrsEye;
            Debug.Log("CreateEye:" + nm + (eyes == null));
        }

        void OnPreRenderListener(int cacheTextureId, VrsViewer.Eye eyeType)
        {
            if (VrsGlobal.isVR9Platform) return;
            if (VrsViewer.USE_DTR && VrsGlobal.supportDtr)
            {
                
                ViarusRenderEventType eventType = eyeType == VrsViewer.Eye.Left ? ViarusRenderEventType.LeftEyeBeginFrame : ViarusRenderEventType.RightEyeBeginFrame;
                VrsPluginEvent.IssueWithData(eventType, cacheTextureId);
                if (VrsGlobal.DEBUG_LOG_ENABLED) Debug.Log("OnPreRender.eye[" + eyeType + "]");
            }
        }

        void OnPostRenderListener(int cacheTextureId, VrsViewer.Eye eyeType)
        {
            if (VrsGlobal.isVR9Platform)
            {
                if (eyeType == VrsViewer.Eye.Right && Application.isMobilePlatform)
                {
                    if (VrsGlobal.DEBUG_LOG_ENABLED) Debug.Log("OnPostRenderListener.PrepareFrame.Right");
                    VrsPluginEvent.Issue(ViarusRenderEventType.PrepareFrame);
                }
                return;
            }

            if (VrsViewer.USE_DTR && VrsGlobal.supportDtr)
            {
                
                ViarusRenderEventType eventType = eyeType == VrsViewer.Eye.Left ? ViarusRenderEventType.LeftEyeEndFrame : ViarusRenderEventType.RightEyeEndFrame;
                
                
                VrsPluginEvent.IssueWithData(eventType, cacheTextureId);
                if(VrsGlobal.DEBUG_LOG_ENABLED) Debug.Log("OnPostRender.eye[" + eyeType + "]");
            }

            if (VrsViewer.USE_DTR && eyeType == VrsViewer.Eye.Right)
            {
                
            }
        }

        void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginCameraRendering += CameraPreRender;
            }
#endif
            StartCoroutine("EndOfFrame");

        }

        void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginCameraRendering -= CameraPreRender;
            }
#endif
            StopCoroutine("EndOfFrame");

        }

#if UNITY_2019_1_OR_NEWER
        public void CameraPreRender(ScriptableRenderContext context, Camera mcam)
        {
            if (mcam.gameObject == cam.gameObject)
            {
                OnPreCull();
            }
        }
#endif

        void OnPreCull()
        {
            if (VrsViewer.Instance.SplitScreenModeEnabled)
            {
                
                VrsEye[] eyes = Eyes;
                for (int i = 0, n = eyes.Length; i < n; i++)
                {
                    if(!eyes[i].cam.enabled) eyes[i].cam.enabled = true;
                }
                
                
                
#if VRS_HACK
#warning Due to a Unity bug, a worldspace canvas in a camera that renders to a RenderTexture allocates infinite memory. Remove the hack ASAP as the fix gets released.
                BlackOutMonoCamera();
#else
                if (!VrsViewer.Instance.IsWinPlatform) cam.enabled = false;
#endif
                renderedStereo = true;
            }

        }

        public void EndOfFrameCore()
        {
            
            if (renderedStereo)
            {
#if VRS_HACK
                RestoreMonoCamera();
#else
                    cam.enabled = true;
#endif
                renderedStereo = false;
            }
        }
 
        IEnumerator EndOfFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                EndOfFrameCore();
            }
        }

#if VRS_HACK
        private CameraClearFlags m_MonoCameraClearFlags;
        private Color m_MonoCameraBackgroundColor;
        private int m_MonoCameraCullingMask;

        private void BlackOutMonoCamera()
        {   
            if (VrsViewer.Instance.IsWinPlatform) return;
            m_MonoCameraClearFlags = cam.clearFlags;
            m_MonoCameraBackgroundColor = cam.backgroundColor;
            m_MonoCameraCullingMask = cam.cullingMask;

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.cullingMask = 0;
        }

        private void RestoreMonoCamera()
        {
            if (VrsViewer.Instance.IsWinPlatform) return;
            cam.clearFlags = m_MonoCameraClearFlags;
            cam.backgroundColor = m_MonoCameraBackgroundColor;
            cam.cullingMask = m_MonoCameraCullingMask;
        }
#endif
    }


}
