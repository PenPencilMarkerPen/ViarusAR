













using UnityEngine;
using UnityEngine.Rendering;











namespace Vrs.Internal
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("VRS/Internal/VrsEye")]
    public class VrsEye : MonoBehaviour
    {
        public delegate void OnPostRenderCallback(int cacheTextureId, VrsViewer.Eye eyeType);
        public OnPostRenderCallback OnPostRenderListener;

        public delegate void OnPreRenderCallback(int cacheTextureId, VrsViewer.Eye eyeType);
        public OnPreRenderCallback OnPreRenderListener;


        
        
        
        public VrsViewer.Eye eye;

        
        
        public VrsStereoController Controller
        {
            
            get
            {
                if (transform.parent == null)
                { 
                    return null;
                }
                if ((Application.isEditor && !Application.isPlaying) || controller == null)
                {
                    
                    controller = transform.parent.GetComponentInParent<VrsStereoController>();
                    if (controller == null)
                    {
                        controller = FindObjectOfType<VrsStereoController>();
                    }
                }
                return controller;
            }
            set
            {
                controller = value;
            }
        }

        private VrsStereoController controller;
        private StereoRenderEffect stereoEffect;
        private Camera monoCamera;

        
        public Camera cam { get; private set; }

        public Transform cacheTransform;
        void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void Start()
        {
            var ctlr = Controller;
            if (ctlr == null)
            {
                Debug.LogError("VrsEye must be child of a StereoController.");
                enabled = false;
                return;
            }
            
            controller = ctlr;
            monoCamera = controller.GetComponent<Camera>();
            cacheTransform = transform;
        }

        public void UpdateCameraProjection()
        {
            if (VrsGlobal.hasInfinityARSDK) return;
            
            Matrix4x4 proj = VrsViewer.Instance.Projection(eye);
            Debug.Log("VrsEye->UpdateCameraProjection," + eye.ToString() + "/" + proj.ToString());
            bool useDFT = VrsViewer.USE_DTR && !VrsGlobal.supportDtr;
            
            if (!VrsViewer.Instance.IsWinPlatform && (Application.isEditor || useDFT))
            {
                if (monoCamera == null) monoCamera = controller.GetComponent<Camera>();
                
                float nearClipPlane = monoCamera.nearClipPlane;
                float farClipPlane = monoCamera.farClipPlane;

                float near = (VrsGlobal.fovNear >= 0 && VrsGlobal.fovNear < nearClipPlane) ? VrsGlobal.fovNear : nearClipPlane;
                float far = (VrsGlobal.fovFar >= 0 && VrsGlobal.fovFar > farClipPlane) ? VrsGlobal.fovFar : farClipPlane;
                
                Debug.Log(eye.ToString() + ", " + cam.rect.ToString());
                VrsCameraUtils.FixProjection(cam.rect, near, far, ref proj);
            }

            
            cam.projectionMatrix = proj;
            VrsViewer.Instance.UpdateEyeCameraProjection(eye);

            float ipd = VrsViewer.Instance.Profile.viewer.lenses.separation;
            Vector3 localPosition = (eye == VrsViewer.Eye.Left ? -ipd / 2 : ipd / 2) * Vector3.right;
            if (localPosition.x != transform.localPosition.x)
            {
                transform.localPosition = localPosition;
            }

            BaseARDevice vrsDevice = VrsViewer.Instance.GetDevice();
            if (vrsDevice != null && vrsDevice.IsSptEyeLocalRotPos())
            {
                transform.localRotation = vrsDevice.GetEyeLocalRotation(eye);
                transform.localPosition = vrsDevice.GetEyeLocalPosition(eye);
                Debug.Log(eye + ". Local Rotation : " + transform.localRotation.eulerAngles.ToString());
                Debug.Log(eye + ". Local Position : " + transform.localPosition.x + "," + transform.localPosition.y + "," + transform.localPosition.z);
            }
        }

        private int cacheTextureId = -1;

        public int GetTargetTextureId()
        {
            return cacheTextureId;
        }

        public void UpdateTargetTexture()
        {
            
            int eyeType = eye == VrsViewer.Eye.Left ? 0 : 1;
            cacheTextureId = VrsViewer.Instance.GetEyeTextureId(eyeType);
            cam.targetTexture = VrsViewer.Instance.GetStereoScreen(eyeType);
            cam.targetTexture.DiscardContents();
        }

        void OnPreRender()
        {
            if (cacheTextureId == -1 && cam.targetTexture != null)
            {
                cacheTextureId = (int)cam.targetTexture.GetNativeTexturePtr();
            }
            if(OnPreRenderListener != null) OnPreRenderListener(cacheTextureId, eye);
        }

        int frameId = 0;
        void OnPostRender()
        {
            if (cacheTextureId == -1 && cam.targetTexture != null)
            {
                cacheTextureId = (int)cam.targetTexture.GetNativeTexturePtr();
            }
            if(OnPostRenderListener != null) OnPostRenderListener(cacheTextureId, eye);

            if (eye == VrsViewer.Eye.Left)
            {
                
                RenderTexture stereoScreen = cam.targetTexture;
                if (stereoScreen != null && VrsViewer.Instance.GetViarusService() != null)
                {
                    int textureId = (int)stereoScreen.GetNativeTexturePtr();
                    bool isCapturing = VrsViewer.Instance.GetViarusService().CaptureDrawFrame(textureId, frameId);
                    if (isCapturing)
                    {
                        GL.InvalidateState();
                    }
                    frameId++;
                }
            }
        }

        private void SetupStereo()
        {
            int eyeType = eye == VrsViewer.Eye.Left ? 0 : 1;
            if (cam.targetTexture == null
                 && VrsViewer.Instance.GetStereoScreen(eyeType) != null)
            {
                cam.targetTexture = monoCamera.targetTexture ?? VrsViewer.Instance.GetStereoScreen(eyeType);
            }
        }

        void OnPreCull()
        {
            if (VrsGlobal.DEBUG_LOG_ENABLED) Debug.Log(eye+".VrsEye.OnPreCull." + cam.rect.ToString());
            if (VrsGlobal.isVR9Platform)
            {
                cam.targetTexture = null;
                return;
            } else
            {
                
                if (!VrsViewer.Instance.SplitScreenModeEnabled)
                {
                    
                    cam.enabled = false;
                    return;
                }

                SetupStereo();

                int eyeType = eye == VrsViewer.Eye.Left ? 0 : 1;
                if (VrsGlobal.DEBUG_LOG_ENABLED) Debug.Log("OnPreCull.eye[" + eyeType + "]");

                if (VrsViewer.Instance.OpenEffectRender && VrsViewer.Instance.GetStereoScreen(eyeType) != null)
                {
                    
                    
                    stereoEffect = GetComponent<StereoRenderEffect>();
                    if (stereoEffect == null)
                    {
                        stereoEffect = gameObject.AddComponent<StereoRenderEffect>();
#if UNITY_5_6_OR_NEWER
                        stereoEffect.UpdateEye(eye);
#endif  
                    }
                    stereoEffect.enabled = true;
                }
                else if (stereoEffect != null)
                {
                    
                    stereoEffect.enabled = false;
                }
            }
        }

        
        
        
        
        public void CopyCameraAndMakeSideBySide(VrsStereoController controller,
                                              float parx = 0, float pary = 0)
        {
#if UNITY_EDITOR
            
            
            var cam = GetComponent<Camera>();
#endif

            float ipd = VrsViewer.Instance.Profile.viewer.lenses.separation;
            Vector3 localPosition = (eye == VrsViewer.Eye.Left ? -ipd / 2 : ipd / 2) * Vector3.right;

            if (monoCamera == null)
            {
                monoCamera = controller.GetComponent<Camera>();
            }

            
            cam.CopyFrom(monoCamera);
#if UNITY_5_6_OR_NEWER
            cam.allowHDR = false;
            cam.allowMSAA = false;
            
#endif
            monoCamera.useOcclusionCulling = false;

            
            
            cam.depth = eye == VrsViewer.Eye.Left ? monoCamera.depth + 1 : monoCamera.depth + 2;

            
            
            transform.localPosition = localPosition;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            Rect left = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
            Rect right = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
            if (eye == VrsViewer.Eye.Left)
                cam.rect = left;
            else
                cam.rect = right;

            
            if (!VrsGlobal.isVR9Platform && VrsViewer.USE_DTR && VrsGlobal.supportDtr && Application.platform == RuntimePlatform.Android)
            {
                
                cam.rect = new Rect(0, 0, 1, 1);
            }

            if (cam.farClipPlane < VrsGlobal.fovFar)
            {
                cam.farClipPlane = VrsGlobal.fovFar;
            }

            if(VrsGlobal.isVR9Platform)
            {
                
                cam.clearFlags = CameraClearFlags.Nothing;
                monoCamera.clearFlags = CameraClearFlags.Nothing;
            }

#if VIARUS_VR
            cam.aspect = 1.0f;
#endif

            Debug.Log(eye.ToString() + "," + cam.transform.localPosition.x);

        }
#if UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
        public Material _flipMat;
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if(_flipMat == null)
            {
                Debug.Log((eye == VrsViewer.Eye.Left ? "Materials/LeftEyeFlip" : "Materials/RightEyeFlip"));
                _flipMat = Resources.Load<Material>(eye == VrsViewer.Eye.Left ? "Materials/LeftEyeFlip" : "Materials/RightEyeFlip");
            }
            Graphics.Blit(source, destination, _flipMat);
        }
#endif

        private void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginCameraRendering += CameraPreRender;
                RenderPipelineManager.endCameraRendering += CameraPostRender;
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                RenderPipelineManager.beginCameraRendering -= CameraPreRender;
                RenderPipelineManager.endCameraRendering -= CameraPostRender;
            }
#endif
        }

#if UNITY_2019_1_OR_NEWER
        public void CameraPreRender(ScriptableRenderContext context, Camera mcam)
        {
            
            if (mcam.gameObject != this.gameObject)
                return;
            OnPreCull();
            OnPreRender();
        }

        public void CameraPostRender(ScriptableRenderContext context, Camera mcam)
        {
            if (mcam.gameObject != this.gameObject)
                return;
            OnPostRender();
        }
#endif

    }
}
