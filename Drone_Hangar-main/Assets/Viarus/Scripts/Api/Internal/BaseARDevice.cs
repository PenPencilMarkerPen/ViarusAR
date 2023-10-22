













#if !UNITY_EDITOR
#if UNITY_ANDROID
#define ANDROID_DEVICE
#endif
#endif

using UnityEngine;
using System.Collections.Generic;
using System;


namespace Vrs.Internal
{
    
    public abstract class BaseARDevice
    {
        private static BaseARDevice device = null;


        protected BaseARDevice()
        {
            Profile = VrsProfile.Default.Clone();
        }

        public VrsProfile Profile { get; protected set; }

        public abstract void Init();

        public abstract void SetSplitScreenModeEnabled(bool enabled);

        public virtual void AndroidLog(string msg) { Debug.Log(msg); }

        public virtual void SetSystemParameters(string key, string value) { }

        public virtual void SetIsKeepScreenOn(bool keep) { }

        public virtual bool SupportsNativeDistortionCorrection(List<string> diagnostics)
        {
            return true;
        }

        public virtual void SetTextureSizeNative(int w, int h) { }

        public virtual void SetCpuLevel(VrsOverrideSettings.PerfLevel level) { }

        public virtual void SetGpuLevel(VrsOverrideSettings.PerfLevel level) { }

        public long viarusVRServiceId;

        public virtual RenderTexture CreateStereoScreen(int w, int h)
        {
            int width = w > 0 ? w : (int)recommendedTextureSize[0];
            int height = h > 0 ? h : (int)recommendedTextureSize[1];
            width = width == 0 ? Screen.width : width;
            height = height == 0 ? Screen.height : height;
            
            bool useDFT = VrsViewer.USE_DTR && !VrsGlobal.supportDtr;
            float DFT_TextureScale = 0.8f;
            if (useDFT)
            {
                TextureQuality textureQuality = VrsViewer.Instance.TextureQuality;

                if (VrsGlobal.offaxisDistortionEnabled)
                {
                    
                }

                if (textureQuality == TextureQuality.Best)
                {
                    DFT_TextureScale = 1f;
                }
                else if (textureQuality == TextureQuality.Good)
                {
                    DFT_TextureScale = 0.75f;
                }
                else if (textureQuality == TextureQuality.Simple)
                {
                    DFT_TextureScale = 0.6666666666666666f;
                }
                else if (textureQuality == TextureQuality.Better)
                {
                    DFT_TextureScale = 0.8f;
                }

                width = (int)(width * DFT_TextureScale);
                height = (int)(height * DFT_TextureScale);

            }
           
            Debug.Log("antiAliasing."+QualitySettings.antiAliasing + "," + (int)VrsViewer.Instance.TextureMSAA);
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.Default);
            rt.anisoLevel = 0;
            int antiAliasing = Mathf.Max(QualitySettings.antiAliasing, (int)VrsViewer.Instance.TextureMSAA);
            if (VrsGlobal.isVR9Platform)
            {
                antiAliasing = 1;
            }
            rt.antiAliasing = antiAliasing < 1 ? 1 : antiAliasing;
            rt.Create();


            VrsViewer.Instance.AndroidLog("Creating ss tex "
               + width + " x " + height + "." + "sInfo : [" + Screen.width + "," + Screen.height + "].DFT_TexScal=" + DFT_TextureScale
               + ",TexQuality=" + VrsViewer.Instance.TextureQuality.ToString() + ", Id=" + rt.GetNativeTexturePtr() + ", MSAA=" + rt.antiAliasing);

            return rt;
        }

        public virtual long CreateViarusVRService()
        {
            return 0;
        }

        public virtual void SetCameraNearFar(float near, float far)
        {

        }


        public virtual void StartCapture(string filePath, int seconds)
        {
            Debug.Log("StartCapture_" + filePath + "_" + seconds);
        }

        public virtual void StopCapture()
        {
        }

        public virtual void OnDrawFrameCapture(int frameId)
        {
        }

        public virtual void SetDisplayQuality(int level)
        {
        }

        public virtual IntPtr NGetRenderEventFunc() { return IntPtr.Zero; }

        public virtual void NIssuePluginEvent(int eventID) { }

        public virtual int GetTimewarpViewNumber()
        {
            return 0;
        }

        public Pose3D GetHeadPose()
        {
            return this.headPose;
        }

        protected MutablePose3D headPose = new MutablePose3D();

        public Matrix4x4 GetProjection(VrsViewer.Eye eye,
                                       VrsViewer.Distortion distortion = VrsViewer.Distortion.Distorted)
        {
            switch (eye)
            {
                case VrsViewer.Eye.Left:
                    return distortion == VrsViewer.Distortion.Distorted ?
                        leftEyeDistortedProjection : leftEyeUndistortedProjection;
                case VrsViewer.Eye.Right:
                    return distortion == VrsViewer.Distortion.Distorted ?
                        rightEyeDistortedProjection : rightEyeUndistortedProjection;
                default:
                    return Matrix4x4.identity;
            }
        }
        protected Matrix4x4 leftEyeDistortedProjection;
        protected Matrix4x4 rightEyeDistortedProjection;
        protected Matrix4x4 leftEyeUndistortedProjection;
        protected Matrix4x4 rightEyeUndistortedProjection;

        public Rect GetViewport(VrsViewer.Eye eye,
                                VrsViewer.Distortion distortion = VrsViewer.Distortion.Distorted)
        {
            switch (eye)
            {
                case VrsViewer.Eye.Left:
                    return distortion == VrsViewer.Distortion.Distorted ?
                        leftEyeDistortedViewport : leftEyeUndistortedViewport;
                case VrsViewer.Eye.Right:
                    return distortion == VrsViewer.Distortion.Distorted ?
                        rightEyeDistortedViewport : rightEyeUndistortedViewport;
                default:
                    return new Rect();
            }
        }
        protected Rect leftEyeDistortedViewport;
        protected Rect rightEyeDistortedViewport;
        protected Rect leftEyeUndistortedViewport;
        protected Rect rightEyeUndistortedViewport;

        protected Vector2 recommendedTextureSize;
        protected int leftEyeOrientation;
        protected int rightEyeOrientation;
 
        public bool profileChanged;

        public abstract void UpdateState();

        public abstract void UpdateScreenData();

        public abstract void Recenter();

        public abstract void PostRender(RenderTexture stereoScreen);

        public virtual void OnPause(bool pause)
        {
            if (!pause)
            {
                UpdateScreenData();
            }
        }

        public virtual void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                UpdateScreenData();
            }
        }
        public virtual void EnterARMode() { }

        public virtual void OnFocus(bool focus)
        {
            
        }

        public virtual void OnApplicationQuit()
        {
            
        }

        public virtual void AppQuit() { }

        public virtual ViarusService GetViarusService()
        {
            return null;
        }

        public virtual string GetStoragePath() { return null; }

        public virtual void ShowVideoPlayer(string path, int type2D3D, int mode, int decode) { }

        public virtual void SetTimeWarpEnable(bool enabled) { }

        

        

        

        

        public virtual void SetIpd(float ipd) { }
        
        
        
        
        public virtual void NSetSystemSplitMode(int flag) { }

        
        
        
        public virtual void NLockTracker() { }

        
        
        
        public virtual void NUnLockTracker() { }

        
        
        
        
        
        public virtual bool GazeApi(GazeTag tag, String param) { return false; }


        public virtual void Destroy()
        {
            if (device == this)
            {
                device = null;
            }
        }

        protected void ComputeEyeFrustum(VrsViewer.Eye eyeType, float near, float far, float left, float right, float top, float bottom)
        {
            if (eyeType == VrsViewer.Eye.Left)
            {
                leftEyeDistortedProjection = MakeProjection(left, top, right, bottom, near, far);
                leftEyeUndistortedProjection = MakeProjection(left, top, right, bottom, near, far);
            }
            else
            {
                rightEyeDistortedProjection = MakeProjection(left, top, right, bottom, near, far);
                rightEyeUndistortedProjection = MakeProjection(left, top, right, bottom, near, far);
            }
        }


        public void ComputeEyesForWin(VrsViewer.Eye eyeType, float near, float far, float left, float top, float right, float bottom)
        {
            Debug.Log("ComputeEyesForWin:" + eyeType + " | near=" + near + ",far=" + far + ",left=" + left + ",right=" + right + ",bottom=" + bottom + ",top=" + top);
            if (eyeType == VrsViewer.Eye.Left)
            {
                leftEyeUndistortedProjection = MakeProjection(left, top, right, bottom, near, far);
                leftEyeDistortedProjection = leftEyeUndistortedProjection;
            }
            else
            {
                rightEyeUndistortedProjection = MakeProjection(left, top, right, bottom, near, far);
                rightEyeDistortedProjection = rightEyeUndistortedProjection;
            }
            recommendedTextureSize = new Vector2(Screen.width, Screen.height);
        }

        
        protected void ComputeEyesFromProfile(float near, float far)
        {
            
            Matrix4x4 leftEyeView = Matrix4x4.identity;
            leftEyeView[0, 3] = -Profile.viewer.lenses.separation / 2;

            float[] rect = new float[4];
            Profile.GetLeftEyeVisibleTanAngles(rect);
            leftEyeDistortedProjection = MakeProjection(rect[0], rect[1], rect[2], rect[3], near, far);
            Profile.GetLeftEyeNoLensTanAngles(rect);
            leftEyeUndistortedProjection = MakeProjection(rect[0], rect[1], rect[2], rect[3], near, far);

            Debug.Log("ComputeEyesFromProfile." + near + "->" + far + "," + rect[0] + "," + rect[1]
                + "," + rect[2] + "," + rect[3]);

            leftEyeUndistortedViewport = Profile.GetLeftEyeVisibleScreenRect(rect);
            leftEyeDistortedViewport = leftEyeUndistortedViewport;

            
            Matrix4x4 rightEyeView = leftEyeView;
            rightEyeView[0, 3] *= -1;

            rightEyeDistortedProjection = leftEyeDistortedProjection;
            rightEyeDistortedProjection[0, 2] *= -1;
            rightEyeUndistortedProjection = leftEyeUndistortedProjection;
            rightEyeUndistortedProjection[0, 2] *= -1;

            rightEyeUndistortedViewport = leftEyeUndistortedViewport;
            rightEyeUndistortedViewport.x = 1 - rightEyeUndistortedViewport.xMax;
            rightEyeDistortedViewport = rightEyeUndistortedViewport;

            if (VrsViewer.USE_DTR) return;

            float width = Screen.width * (leftEyeUndistortedViewport.width + rightEyeDistortedViewport.width);
            float height = Screen.height * Mathf.Max(leftEyeUndistortedViewport.height,
                                                     rightEyeUndistortedViewport.height);
            recommendedTextureSize = new Vector2(width, height);
            Debug.Log("recommendedTextureSize: " + width + "," + height);
        }

        public static Matrix4x4 MakeProjection(float l, float t, float r, float b, float n, float f)
        {
            Matrix4x4 m = Matrix4x4.zero;
            m[0, 0] = 2 * n / (r - l);
            m[1, 1] = 2 * n / (t - b);
            m[0, 2] = (r + l) / (r - l);
            m[1, 2] = (t + b) / (t - b);
            m[2, 2] = (n + f) / (n - f);
            m[2, 3] = 2 * n * f / (n - f);
            m[3, 2] = -1;
            return m;
        }

        
        
        
        public virtual void TurnOff() { }
        public virtual void Reboot() { }
        
        public static BaseARDevice GetDevice()
        {
            if (device == null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                device = new EditorDevice();
#elif ANDROID_DEVICE
        device = new AndroidDevice();
#else
        throw new InvalidOperationException("Unsupported device.");
#endif
            }
            return device;
        }

        public virtual void SetColorspaceType(int colorSpace)
        {

        }

        public virtual void SetControllerSupportMode(ControllerSupportMode csm)
        {

        }

        public virtual VrsInstantNativeApi.ViarusDeviceType GetSixDofControllerPrimaryDeviceType()
        {
            return VrsInstantNativeApi.ViarusDeviceType.None;
        }

        public virtual void SetSixDofControllerPrimaryDeviceType(VrsInstantNativeApi.ViarusDeviceType deviceType)
        {
            
        }

        public virtual int GetControllerTipState()
        {
            return 0;
        }

        public virtual void SetControllerTipState(int state)
        {
            
        }
        
        public virtual int GetEnableSystemDialog()
        {
            return 0;
        }

        public virtual void SetEnableSystemDialog(int enableSystemDialog)
        {
            
        }

        public virtual int GetSystemUIControl()
        {
            return 0;
        }
        
        public virtual void SetMultiThreadedRendering(bool isMultiThreadedRendering)
        {

        }
        
        public virtual bool IsHeadPoseUpdated()
        {
            return false;
        }

        public virtual bool IsSptEyeLocalRotPos()
        {
            return false;
        }

        public virtual Quaternion GetEyeLocalRotation(VrsViewer.Eye eye)
        {
            return Quaternion.identity;
        }

        public virtual Vector3 GetEyeLocalPosition(VrsViewer.Eye eye)
        {
            return Vector3.zero;
        }

        public virtual bool GetProjectionScreenStatus()
        {
            return false;
        }

        public virtual void GetWfdScreenSize(int[] screenSize)
        {
            
        }
    }
}



