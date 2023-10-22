













using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace Vrs.Internal
{
    public abstract class VrsDevice :
#if UNITY_ANDROID
  BaseAndroidDevice
#else
  BaseARDevice
#endif
    {
         
        
        private const int kRenderEvent = 0x47554342;

        
        
        private const int kTilted = 1 << 1;
        private const int kProfileChanged = 1 << 2;
        private const int kVRBackButtonPressed = 1 << 3;

        private float[] position = new float[3];
        private float[] headData = new float[16];
        private float[] viewData = new float[16 * 6 + 12];
        private float[] profileData = new float[15];

        private Matrix4x4 headView = new Matrix4x4();
        private Matrix4x4 leftEyeView = new Matrix4x4();
        private Matrix4x4 rightEyeView = new Matrix4x4();

        private int _timewarp_view_number = 0;
        private bool isHeadPoseUpdated = false;

        public override ViarusService GetViarusService()
        {
            return VrsGlobal.viarusService;
        }

        public override void Init()
        { 
            
            byte[] version = System.Text.Encoding.UTF8.GetBytes(Application.unityVersion);
            if (VrsViewer.USE_DTR)
            {  
                if (!VrsGlobal.nvrStarted)
                {
                    if (viarusVRServiceId == 0)
                    {
                        
                        viarusVRServiceId = CreateViarusVRService(); 
                    }
                    _NVR_InitAPIs(VrsGlobal.useNvrSo);
                    _NVR_SetUnityVersion(version, version.Length);
                    _NVR_Start(viarusVRServiceId);
                    SetDisplayQuality((int) VrsViewer.Instance.TextureQuality);
                    SetMultiThreadedRendering(SystemInfo.graphicsMultiThreaded);
                    Debug.LogError("graphicsMultiThreaded=" + SystemInfo.graphicsMultiThreaded);
                    
                    if (VrsGlobal.soVersion >= 361)
                    {
                        ColorSpace colorSpace = QualitySettings.activeColorSpace;
                        if (colorSpace == ColorSpace.Gamma)
                        {
                            Debug.Log("Color Space - Gamma");
                            SetColorspaceType(0);
                        }
                        else if (colorSpace == ColorSpace.Linear)
                        {
                            Debug.Log("Color Space - Linear");
                            SetColorspaceType(1);
                        }
                    } else
                    {
                        Debug.LogError("System Api Not Support ColorSpace!!!");
                    }

                    if (VrsGlobal.soVersion >= 365)
                    {
                        Debug.Log("Controller Support Mode - " + VrsViewer.Instance.controllerSupportMode.ToString());
                        SetControllerSupportMode(VrsViewer.Instance.controllerSupportMode);
                    }
                    VrsGlobal.nvrStarted = true;
                    
                    ViarusService viarusService = new ViarusService();
                    viarusService.Init();
                    VrsGlobal.viarusService = viarusService;

                    
                    VrsSDKApi.Instance.IsSptEyeLocalRp = IsSptEyeLocalRotPos();
                    if (VrsSDKApi.Instance.IsSptEyeLocalRp)
                    {
                        _NVR_GetEyeLocalRotPos(VrsSDKApi.Instance.LeftEyeLocalRotation,
                            VrsSDKApi.Instance.LeftEyeLocalPosition, VrsSDKApi.Instance.RightEyeLocalRotation,
                            VrsSDKApi.Instance.RightEyeLocalPosition);
                    }
                } 
            } 
           Debug.Log("VrsDevice->Init.isSptEyeLocalRp=" + VrsSDKApi.Instance.IsSptEyeLocalRp);
        }

        public override int GetTimewarpViewNumber()
        {
            return _timewarp_view_number;
        }

        public override bool IsHeadPoseUpdated()
        {
            return isHeadPoseUpdated;
        }

        public override void UpdateState()
        {
            if (VrsViewer.USE_DTR)
            {
                _NVR_GetHeadPoseAndPosition(position, headData , ref _timewarp_view_number);
                VrsSDKApi.Instance.HeadPosition = new Vector3(position[0], position[1], position[2]);
                

                if(VrsViewer.Instance.SixDofMode == SixDofMode.Head_3Dof_Ctrl_3Dof || 
                    VrsViewer.Instance.SixDofMode == SixDofMode.Head_3Dof_Ctrl_6Dof)
                {
                    position[0] = 0.0f;
                    position[1] = 0.0f;
                    position[2] = 0.0f;
                }

                if(position[0] != 0 || position[1] != 0 || position[2] != 0)
                {
                    if(VrsViewer.onSixDofPosition != null)
					   VrsViewer.onSixDofPosition(position[0], position[1], position[2]);
                }
            }

            
            if ((VrsGlobal.verifyStatus >= 0 && VrsGlobal.verifyStatus != VERIFY_STATUS.SUCC) || VrsViewer.Instance.LockHeadTracker || (headData[0] == 0 && headData[15] == 0))
            {
                headData = new float[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            } 

            if(VrsGlobal.verifyStatus >= 0 && VrsGlobal.verifyStatus != VERIFY_STATUS.SUCC)
            {
                AndroidLog("------------------------------Verify Failed : " +VrsGlobal.verifyStatus + "------------------------------");
            }

            ExtractMatrix(ref headView, headData);
            headPose.SetRightHanded(headView);
            headPose.SetPosition(new Vector3(position[0], position[1], position[2]));
            isHeadPoseUpdated = true;
        }

        public override void UpdateScreenData()
        {
            bool useDFT = VrsViewer.USE_DTR && !VrsGlobal.supportDtr;
            
            UpdateProfile();
            UpdateView();
            

            if(useDFT || VrsViewer.Instance.IsWinPlatform)
            {
                float far = VrsGlobal.fovFar > -1 ? VrsGlobal.fovFar : (Camera.main != null ? Camera.main.farClipPlane : 20000.0f);
                ComputeEyesFromProfile(1, far);
            }
           
            profileChanged = true;
        }

        public override void Recenter()
        {
            if (VrsViewer.USE_DTR)
            {
                _NVR_ResetHeadPose();
            }
        }

        public override void PostRender(RenderTexture stereoScreen)
        {
           
        }

        public override void EnterARMode() {
            Debug.Log("VrsDevice->EnterARMode");
            VrsPluginEvent.Issue(ViarusRenderEventType.BeginVR);
            _NVR_ApplicationResume();
            
            UpdateScreenData();
        }

        public override void OnApplicationPause(bool pause)
        {
            Debug.Log("VrsDevice->OnApplicationPause." + pause);
            base.OnApplicationPause(pause);
            
            if (pause)
            {
                Debug.Log("VrsDevice->OnPause");
                if (VrsViewer.USE_DTR)
                {
                    VrsSDKApi.Instance.IsIVrsMode = false;
                    VrsPluginEvent.Issue(ViarusRenderEventType.EndVR);
                    _NVR_ApplicationPause();
                }
            }
            else
            {
                Debug.Log("VrsDevice->OnResume");
                if (VrsViewer.USE_DTR)
                {
                    VrsSDKApi.Instance.IsIVrsMode = true;
                    VrsPluginEvent.Issue(ViarusRenderEventType.BeginVR);
                    _NVR_ApplicationResume();
                    UpdateScreenData();
                }
            }
        }

        public override void Destroy()
        {
            Debug.Log("VrsDevice->Destroy");
            base.Destroy();
        }

        private bool applicationQuited = false;
        public override void OnApplicationQuit()
        {
            if (VrsViewer.USE_DTR && !applicationQuited)
            {  
                Input.gyro.enabled = false;
                VrsPluginEvent.Issue(ViarusRenderEventType.ShutDown);
                _NVR_ApplicationDestory();
            } 
            applicationQuited = true;
            base.OnApplicationQuit();
            Debug.Log("VrsDevice->OnApplicationQuit.");
        }

        private void UpdateView()
        {
            if (VrsViewer.USE_DTR)
            {
                _NVR_GetViewParameters(viewData);
            }

            int j = 0; 

            j = ExtractMatrix(ref leftEyeView, viewData, j);
            j = ExtractMatrix(ref rightEyeView, viewData, j);
            if (VrsViewer.USE_DTR)
            {
                
                leftEyeView = leftEyeView.transpose;
                rightEyeView = rightEyeView.transpose;
            }
            
            

            j = ExtractMatrix(ref leftEyeDistortedProjection, viewData, j);
            j = ExtractMatrix(ref rightEyeDistortedProjection, viewData, j);
            j = ExtractMatrix(ref leftEyeUndistortedProjection, viewData, j);
            j = ExtractMatrix(ref rightEyeUndistortedProjection, viewData, j);
            if (VrsViewer.USE_DTR)
            {
                
                leftEyeDistortedProjection = leftEyeDistortedProjection.transpose;
                rightEyeDistortedProjection = rightEyeDistortedProjection.transpose;
                leftEyeUndistortedProjection = leftEyeUndistortedProjection.transpose;
                rightEyeUndistortedProjection = rightEyeUndistortedProjection.transpose;
            }

            leftEyeUndistortedViewport.Set(viewData[j], viewData[j + 1], viewData[j + 2], viewData[j + 3]);
            leftEyeDistortedViewport = leftEyeUndistortedViewport;
            j += 4;

            rightEyeUndistortedViewport.Set(viewData[j], viewData[j + 1], viewData[j + 2], viewData[j + 3]);
            rightEyeDistortedViewport = rightEyeUndistortedViewport;
            j += 4;
            
            int screenWidth = (int)viewData[j];
            int screenHeight = (int)viewData[j + 1];
            j += 2;

            recommendedTextureSize = new Vector2(viewData[j], viewData[j + 1]);
            j += 2;

            if (VrsViewer.USE_DTR && !VrsGlobal.supportDtr) {
                
                recommendedTextureSize = new Vector2(screenWidth, screenHeight);
                Debug.Log("DFT texture size : " +screenWidth + "," + screenHeight);
            }
        }

        private void UpdateProfile()
        {
            if (VrsViewer.USE_DTR)
            {
                _NVR_GetNVRConfig(profileData);
            }

            if (profileData[13] > 0)
            {
                VrsGlobal.fovNear = profileData[13];
            }

            if (profileData[14] > 0)
            {
                VrsGlobal.fovFar = profileData[14] > VrsGlobal.fovFar ? profileData[14] : VrsGlobal.fovFar;
            }


            if (VrsViewer.USE_DTR && !VrsGlobal.supportDtr && VrsGlobal.dftProfileParams[0] != 0)
            {
                
                
                profileData[0] = VrsGlobal.dftProfileParams[3];
                profileData[1] = VrsGlobal.dftProfileParams[4]; 
                profileData[2] = VrsGlobal.dftProfileParams[5]; 
                profileData[3] = VrsGlobal.dftProfileParams[6]; 
                
                profileData[4] = VrsGlobal.dftProfileParams[12]; 
                profileData[5] = VrsGlobal.dftProfileParams[13]; 
                
                profileData[7] = VrsGlobal.dftProfileParams[0]; 
                
                profileData[9] = VrsGlobal.dftProfileParams[2]; 
                
                profileData[11] = VrsGlobal.dftProfileParams[7]; 
                profileData[12] = VrsGlobal.dftProfileParams[8]; 
                if(VrsGlobal.offaxisDistortionEnabled)
                {
                    
                }
            }

            VrsProfile.Viewer device = new VrsProfile.Viewer();
            VrsProfile.Screen screen = new VrsProfile.Screen();
            
            device.maxFOV.outer = profileData[0];
            device.maxFOV.upper = profileData[2];
            device.maxFOV.inner = profileData[1];
            device.maxFOV.lower = profileData[3];
            screen.width = profileData[4];
            screen.height = profileData[5];
            screen.border = profileData[6];
            device.lenses.separation = profileData[7];
            device.lenses.offset = profileData[8];
            device.lenses.screenDistance = profileData[9];
            device.lenses.alignment = (int)profileData[10];
            device.distortion.Coef = new[] { profileData[11], profileData[12] };
            Profile.screen = screen;
            Profile.viewer = device;

            float[] rect = new float[4];
            Profile.GetLeftEyeNoLensTanAngles(rect);
            float maxRadius = VrsProfile.GetMaxRadius(rect);
            Profile.viewer.inverse = VrsProfile.ApproximateInverse(
            Profile.viewer.distortion, maxRadius);
        }

        private static int ExtractMatrix(ref Matrix4x4 mat, float[] data, int i = 0)
        {
            
            
            for (int r = 0; r < 4; r++)
            {
                for (int c = 0; c < 4; c++, i++)
                {
                    mat[r, c] = data[i];
                }
            }
            return i;
        }
         
        public override IntPtr NGetRenderEventFunc() {
            return _NVR_GetRenderEventFunc();
        }

        public override void NSetSystemSplitMode(int flag) {
            _NVR_SetSystemVRMode(flag);
        }

        public override void NLockTracker()
        {
            _NVR_LockHeadPose();
        }

        public override void NUnLockTracker()
        {
            _NVR_UnLockHeadPose();
        }

        public override void SetTextureSizeNative(int w, int h)
        {
            
            _NVR_SetParamI(1002, w * 10000 + h);
        }

        public override void SetCpuLevel(VrsOverrideSettings.PerfLevel level)
        {
            _NVR_SetParamI(1003, (int) level);
        }

        public override void SetGpuLevel(VrsOverrideSettings.PerfLevel level)
        {
            _NVR_SetParamI(1004, (int)level);
        }

        public override void NIssuePluginEvent(int eventID)
        {
            
            GL.IssuePluginEvent(VrsViewer.Instance.GetDevice().NGetRenderEventFunc(), eventID);
        }
		
		public override void SetColorspaceType(int colorSpace)
        {
            _NVR_SetParamI((int)PARAMS_KEY.COLOR_SPACE, colorSpace);
        }

        public override void SetControllerSupportMode(ControllerSupportMode csm)
        {
            _NVR_SetParamI((int)PARAMS_KEY.CONTROLLER_SUPPORT, (int) csm);
        }

        public override void SetMultiThreadedRendering(bool isMultiThreadedRendering)
        {
            _NVR_SetParamI((int)PARAMS_KEY.MULTITHREAD_RENDERING, isMultiThreadedRendering ? 1 : 0);
        }

        public override bool IsSptEyeLocalRotPos()
        {
            return _NVR_GetParamI((int)PARAMS_KEY.EYE_LOCAL_ROT_POS) == 1;
        }

        public override Quaternion GetEyeLocalRotation(VrsViewer.Eye eye)
        {
            float[] eulerAngles = new float[3];
            if(eye == VrsViewer.Eye.Left)
            {
                VrsCameraUtils.RotationMatrixToEulerAngles(ref eulerAngles, VrsSDKApi.Instance.LeftEyeLocalRotation);
            }
            else if(eye == VrsViewer.Eye.Right)
            {
                VrsCameraUtils.RotationMatrixToEulerAngles(ref eulerAngles, VrsSDKApi.Instance.RightEyeLocalRotation);
            }
            return Quaternion.Euler(eulerAngles[0], eulerAngles[1], eulerAngles[2]);
        }

        public override Vector3 GetEyeLocalPosition(VrsViewer.Eye eye)
        {
            if (eye == VrsViewer.Eye.Left)
            {
                return new Vector3(VrsSDKApi.Instance.LeftEyeLocalPosition[0], VrsSDKApi.Instance.LeftEyeLocalPosition[1], VrsSDKApi.Instance.LeftEyeLocalPosition[2]);
            }
            else if (eye == VrsViewer.Eye.Right)
            {
                return new Vector3(VrsSDKApi.Instance.RightEyeLocalPosition[0], VrsSDKApi.Instance.RightEyeLocalPosition[1], VrsSDKApi.Instance.RightEyeLocalPosition[2]);
            }
            return Vector3.zero;
        }

        public override bool GetProjectionScreenStatus()
        {
            return _NVR_GetParamI((int)PARAMS_KEY.UNITY_PARAMS_KEY_WFD_STATUS) == 1;
        }

        public override void GetWfdScreenSize(int[] screenSize)
        {
            _NVR_GetWfdScreenSize(screenSize);
        }

        public enum PARAMS_KEY
        {
            CONTROLLER_SUPPORT=1006,
            COLOR_SPACE = 1007,
            TURN_AROUND_STATE=1008,
            TURN_AROUND_YAWOFFSET=1009,
            MULTITHREAD_RENDERING=1010,
            EYE_LOCAL_ROT_POS = 1011,
            UNITY_PARAMS_KEY_WFD_STATUS = 1012
        }

        
        private const string nvrDllName = "nvr_unity";

        [DllImport(nvrDllName)]
        private static extern int _NVR_InitAPIs(bool supportDTR);

        [DllImport(nvrDllName)]
        private static extern bool _NVR_Start(long pointer);

        [DllImport(nvrDllName)]
        private static extern void _NVR_SetUnityVersion(byte[] version_str, int version_length);

        [DllImport(nvrDllName)]
        private static extern int _NVR_GetEventFlags();

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetNVRConfig(float[] profile);

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetHeadPose(float[] pose, ref int viewNumber);

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetHeadPoseAndPosition(float[] position, float[] pose, ref int viewNumber);

        [DllImport(nvrDllName)]
        private static extern void _NVR_ResetHeadPose();

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetViewParameters(float[] viewParams);

        [DllImport(nvrDllName)]
        private static extern void _NVR_ApplicationPause();

        [DllImport(nvrDllName)]
        private static extern void _NVR_ApplicationResume();

        [DllImport(nvrDllName)]
        private static extern void _NVR_ApplicationDestory();

        [DllImport(nvrDllName)]
        private static extern IntPtr _NVR_GetRenderEventFunc();

        [DllImport(nvrDllName)]
        private static extern void _NVR_LockHeadPose();

        [DllImport(nvrDllName)]
        private static extern void _NVR_UnLockHeadPose();

        [DllImport(nvrDllName)]
        private static extern void _NVR_SetSystemVRMode(int flag);

        [DllImport(nvrDllName)]
        private static extern void _NVR_SetParamI(int key, int value);

        [DllImport(nvrDllName)]
        private static extern int _NVR_GetParamI(int key);

        [DllImport(nvrDllName)]
        private static extern void _NVR_GetEyeLocalRotPos(float[] leftEyeRot, float[] leftEyePos, float[] rightEyeRot, float[] rightEyePos);
        
        [DllImport(nvrDllName)]
        private static extern void _NVR_GetWfdScreenSize(int[] screenSize);
    }
}

