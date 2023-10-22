












#if UNITY_ANDROID

using UnityEngine;


namespace Vrs.Internal
{
    public class AndroidDevice : VrsDevice
    {
        
        private const string ActivityListenerClass =
            "com.nibiru.lib.xr.unity.NibiruVRUnityService";

        
        private const string ViarusVRClass = "com.nibiru.lib.vr.NibiruVR";

        private static AndroidJavaObject activityListener, viarusVR;

        AndroidJavaObject viarusVRService = null;

        public override void Init()
        {
            SetApplicationState();

            ConnectToActivity();
            base.Init();
        }

        protected override void ConnectToActivity()
        {
            base.ConnectToActivity();
            if (androidActivity != null && activityListener == null)
            {
                activityListener = Create(ActivityListenerClass);
            }
            if (androidActivity != null && viarusVR == null)
            {
                viarusVR = Create(ViarusVRClass);
            }
        }

        public override void TurnOff()
        {
            CallStaticMethod(activityListener, "shutdownBroadcast");
        }

        public override void Reboot()
        {
            CallStaticMethod(activityListener, "rebootBroadcast");
        }

        public override long CreateViarusVRService()
        {
            string hmdType = "NONE";
            CallStaticMethod(ref hmdType, viarusVR, "getMetaData", androidActivity, "HMD_TYPE");
            if (hmdType != null)
            {
                VrsViewer.Instance.HmdType = hmdType.Equals("AR") ? HMD_TYPE.AR : (hmdType.Equals("VR") ? HMD_TYPE.VR : HMD_TYPE.NONE);
            }

            string initParams = "";
            long pointer = 0;
            CallStaticMethod(ref initParams, viarusVR, "initNibiruVRServiceForUnity", androidActivity);
            
            Debug.Log("initParams is " + initParams + ",hmdType is " + hmdType);
            string[] data = initParams.Split('_');
            pointer = long.Parse(data[0]);
            VrsGlobal.supportDtr = (int.Parse(data[1]) == 1 ? true : false);
            VrsGlobal.distortionEnabled = (int.Parse(data[2]) == 1 ? true : false);
            VrsGlobal.useNvrSo = (int.Parse(data[3]) == 1 ? true : false);
            if (data.Length >= 5)
            {
                VrsGlobal.offaxisDistortionEnabled = (int.Parse(data[4]) == 1 ? true : false);
            }
            
            if (VrsViewer.Instance.TrackerPosition)
            {
                CallStaticMethod(viarusVR, "setTrackingModeForUnity", (int)TRACKING_MODE.POSITION);
            }


            int meshSizeX = -1;
            if (data.Length >= 6)
            {
                meshSizeX = (int)float.Parse(data[5]);
            }

            int meshSizeY = -1;
            if (data.Length >= 7)
            {
                meshSizeY = (int)float.Parse(data[6]);
            }

            if (data.Length >= 8)
            {
                float fps = float.Parse(data[7]);
                
                VrsGlobal.refreshRate = Mathf.Max(60, fps > 0 ? fps : 0);
            }

            if (meshSizeX > 0 && meshSizeY > 0)
            {
                VrsGlobal.meshSize = new int[] { meshSizeX, meshSizeY };
            }

            string channelCode = "";
            CallStaticMethod<string>(ref channelCode, viarusVR, "getChannelCode");
            VrsGlobal.channelCode = channelCode;

            
            int[] allVersion = new int[] { -1, -1, -1, -1 };
            CallStaticMethod(ref allVersion, viarusVR, "getVersionForUnity");
            VrsGlobal.soVersion = allVersion[0];
            VrsGlobal.jarVersion = allVersion[1];
            VrsGlobal.platPerformanceLevel = allVersion[2];
            VrsGlobal.platformID = allVersion[3];
            VrsSDKApi.Instance.IsSptMultiThreadedRendering = VrsGlobal.soVersion >= 414;
            VrsGlobal.isVR9Platform = VrsGlobal.platformID == (int)PLATFORM.PLATFORM_VR9;
            if (VrsGlobal.isVR9Platform)
            {
                VrsGlobal.distortionEnabled = false;
                VrsGlobal.supportDtr = true;
                VrsViewer.Instance.SwitchControllerMode(false);
            }

            if(!VrsSDKApi.Instance.IsSptMultiThreadedRendering && SystemInfo.graphicsMultiThreaded)
            {
                AndroidLog("*****Warning******\n\n System Does Not Support Unity MultiThreadedRendering !!! \n\n*****Warning******");
                AndroidLog("Support Unity MultiThreadedRendering Need V2 Version >=414, Currently Is " + VrsGlobal.soVersion + " !!!");
            }

            Debug.Log("AndDev->Service : [pointer]=" + pointer + ", [dtrSpt] =" + VrsGlobal.supportDtr + ", [DistEnabled]=" +
            VrsGlobal.distortionEnabled + ", [useNvrSo]=" + VrsGlobal.useNvrSo + ", [code]=" + channelCode + ", [jar]=" + VrsGlobal.jarVersion + ", [so]=" + VrsGlobal.soVersion
            + ", [platform id]=" + VrsGlobal.platformID + ", [pl]=" + VrsGlobal.platPerformanceLevel + ",[offaxisDist]=" + VrsGlobal.offaxisDistortionEnabled + ",[mesh]=" + meshSizeX +
            "*" + meshSizeY + ",[fps]=" + VrsGlobal.refreshRate + "," + channelCode);

            
            string cardboardParams = "";
            CallStaticMethod<string>(ref cardboardParams, viarusVR, "getNibiruVRConfigFull");
            if (cardboardParams.Length > 0)
            {
                Debug.Log("cardboardParams is " + cardboardParams);
                string[] profileData = cardboardParams.Split('_');
                for (int i = 0; i < VrsGlobal.dftProfileParams.Length; i++)
                {
                    if (i >= profileData.Length) break;

                    if (profileData[i] == null || profileData[i].Length == 0) continue;

                    VrsGlobal.dftProfileParams[i] = float.Parse(profileData[i]);
                }
            }
            else
            {
                Debug.Log("Vrs->AndroidDevice->getViaeusVRConfigFull Failed ! ");
            }

            
            if (VrsGlobal.offaxisDistortionEnabled)
            {
                string offaxisParams = "";
                CallStaticMethod<string>(ref offaxisParams, viarusVR, "getOffAxisDistortionConfig");
                if (offaxisParams != null && offaxisParams.Length > 0)
                {
                    VrsGlobal.offaxisDistortionConfigData = offaxisParams;
                    
                }

                string sdkParams = "";
                CallStaticMethod<string>(ref sdkParams, viarusVR, "getSDKConfig");
                if (sdkParams != null && sdkParams.Length > 0)
                {
                    VrsGlobal.sdkConfigData = sdkParams;
                    string[] linesCN = sdkParams.Split('\n');
                    
                    foreach (string line in linesCN)
                    {
                        if (line == null || line.Length <= 1)
                        {
                            continue;
                        }
                        string[] keyAndValue = line.Split('=');
                        
                        if (keyAndValue[0].Contains("oad_offset_x1"))
                        {
                            VrsGlobal.offaxisOffset[0] = int.Parse(keyAndValue[1]);
                        }
                        else if (keyAndValue[0].Contains("oad_offset_x2"))
                        {
                            VrsGlobal.offaxisOffset[1] = int.Parse(keyAndValue[1]);
                        }
                        else if (keyAndValue[0].Contains("oad_offset_y1"))
                        {
                            VrsGlobal.offaxisOffset[2] = int.Parse(keyAndValue[1]);
                        }
                        else if (keyAndValue[0].Contains("oad_offset_y2"))
                        {
                            VrsGlobal.offaxisOffset[3] = int.Parse(keyAndValue[1]);
                        }
                    }
                }

                Debug.Log("Offaxis Offset : " + VrsGlobal.offaxisOffset[0] + "," + VrsGlobal.offaxisOffset[1] + "," + VrsGlobal.offaxisOffset[2] + "," + VrsGlobal.offaxisOffset[3]);
            }

            
            return pointer;
        }

        public override void SetDisplayQuality(int level)
        {
            CallStaticMethod(viarusVR, "setDisplayQualityForUnity", level);
        }

        public override bool GazeApi(GazeTag tag, string param)
        {
            bool show = false;
            CallStaticMethod<bool>(ref show, viarusVR, "gazeApiForUnity", (int)tag, param);
            return show;
        }

        public override void SetSplitScreenModeEnabled(bool enabled)
        {

        }
        public override void AndroidLog(string msg)
        {
            CallStaticMethod(activityListener, "log", msg);
        }
        public override void SetSystemParameters(string key, string value)
        {
            if (viarusVR != null)
            {
                CallStaticMethod(viarusVR, "setSystemParameters", key, value);
            }
        }

        public override void OnApplicationPause(bool pause)
        {
            base.OnApplicationPause(pause);
            
            if (!pause && androidActivity != null)
            {
                RunOnUIThread(androidActivity, new AndroidJavaRunnable(runOnUiThread));
            }

        }

        public override void AppQuit()
        {
            if (androidActivity != null)
            {
                RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
                {
                    androidActivity.Call("finish");
                }));
            }
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }

        void runOnUiThread()
        {
            
            
            AndroidJavaObject androidWindow = androidActivity.Call<AndroidJavaObject>("getWindow");
            androidWindow.Call("addFlags", 128);
            AndroidJavaObject androidDecorView = androidWindow.Call<AndroidJavaObject>("getDecorView");
            androidDecorView.Call("setSystemUiVisibility", 5894);
        }

        public override void SetIsKeepScreenOn(bool keep)
        {
            if (androidActivity != null)
            {
                RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
                {
                    SetScreenOn(keep);
                }));
            }
        }
        
        
        
        
        
        void SetScreenOn(bool enable)
        {
            if (enable)
            {
                AndroidJavaObject androidWindow = androidActivity.Call<AndroidJavaObject>("getWindow");
                androidWindow.Call("addFlags", 128);
            }
            else
            {
                AndroidJavaObject androidWindow = androidActivity.Call<AndroidJavaObject>("getWindow");
                androidWindow.Call("clearFlags", 128);
            }
        }

        private void SetApplicationState()
        {
        }

        
        
        
        
        
        
        public override void ShowVideoPlayer(string path, int type2D3D, int mode, int decode)
        {
            CallStaticMethod(viarusVR, "showVideoPlayer", path, type2D3D, mode, decode);
        }

        
        
        
        

        void InitViarusVRService()
        {
            if (viarusVRService == null)
            {
                
                CallStaticMethod<AndroidJavaObject>(ref viarusVRService, viarusVR, "getNibiruVRService", null);
            }
        }

        public override void SetIpd(float ipd)
        {
            InitViarusVRService();
            if (viarusVRService != null)
            {
                CallObjectMethod(viarusVRService, "setIpd", ipd);
            }
            else
            {
                Debug.LogError("SetIpd failed, because ss is null !!!!");
            }
        }

        public override void SetTimeWarpEnable(bool enabled)
        {
            InitViarusVRService();
            if (viarusVRService != null)
            {
                CallObjectMethod(viarusVRService, "setTimeWarpEnable", enabled);
            }
            else
            {
                Debug.LogError("SetTimeWarpEnable failed, because ss is null !!!!");
            }
        }

        public override string GetStoragePath() { return GetAndroidStoragePath(); }

        public override void SetCameraNearFar(float near, float far)
        {
            CallStaticMethod(viarusVR, "setProjectionNearFarForUnity", near, far);
        }

        public override void StopCapture()
        {
            CallStaticMethod(viarusVR, "stopCaptureForUnity");
        }

        public override void OnDrawFrameCapture(int frameId)
        {
            CallStaticMethod(viarusVR, "onDrawFrameForUnity", frameId);
        }

        public override VrsInstantNativeApi.ViarusDeviceType GetSixDofControllerPrimaryDeviceType()
        {
            string result = "3";
            CallStaticMethod<string>(ref result, viarusVR, "getSystemProperty", "Vrs.ctrl.primaryhand", "3");
            Debug.Log("primaryhand_" + result);
            int type = int.Parse(result);
            
            if (type == 0)
            {
                return VrsInstantNativeApi.ViarusDeviceType.RightController;
            } else if(type == 1)
            {
                return VrsInstantNativeApi.ViarusDeviceType.LeftController;
            }
            return VrsInstantNativeApi.ViarusDeviceType.None;
        }

        public override void SetSixDofControllerPrimaryDeviceType(VrsInstantNativeApi.ViarusDeviceType deviceType)
        {
            int type = -1;
            if(deviceType == VrsInstantNativeApi.ViarusDeviceType.LeftController)
            {
                type = 1;
            } else if(deviceType == VrsInstantNativeApi.ViarusDeviceType.RightController)
            {
                type = 0;
            }

            if (type >=0) CallStaticMethod(viarusVR, "setSystemProperty", "Vrs.ctrl.primaryhand", "" + type);
        }
        
        public override int GetControllerTipState()
        {
            string result = "0";
            CallStaticMethod<string>(ref result, viarusVR, "getSystemProperty", "Vrs.ctrl.calib.tip", "0");
            int state = int.Parse(result);
            
            return state;
        }

        public override void SetControllerTipState(int state)
        {
            CallStaticMethod(viarusVR, "setSystemProperty", "Vrs.ctrl.calib.tip", "" + state);
        }
        
        public override int GetEnableSystemDialog()
        {
            string result = "0";
            CallStaticMethod<string>(ref result, viarusVR, "getSystemProperty", "persist.nsr.sysui.enable", "0");
            int enableSystemDialog = int.Parse(result);
            return enableSystemDialog;
        }

        public override void SetEnableSystemDialog(int enableSystemDialog)
        {
            CallStaticMethod(viarusVR, "setSystemProperty", "persist.nsr.sysui.enable", "" +enableSystemDialog); 
        }

        public override int GetSystemUIControl()
        {
            string result = "0";
            CallStaticMethod<string>(ref result, viarusVR, "getSystemProperty", "Vrs.sysui.control", "0");
            int systemUIControl = int.Parse(result);
            return systemUIControl;
        }
    }
}


#endif
