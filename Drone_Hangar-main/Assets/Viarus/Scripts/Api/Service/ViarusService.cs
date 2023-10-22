using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Vrs.Internal
{
    
    
    
    public class ViarusService
    {
        private const string ViarusSDKClassName = "com.nibiru.lib.vr.NibiruVR";
        protected AndroidJavaObject androidActivity;
        protected AndroidJavaClass viarusSDKClass;
        protected AndroidJavaObject viarusOsServiceObject;
        protected AndroidJavaObject viarusSensorServiceObject;
        protected AndroidJavaObject viarusVoiceServiceObject;
        protected AndroidJavaObject viarusGestureServiceObject;
        protected AndroidJavaObject viarusVRServiceObject;
        protected AndroidJavaObject viarusCameraServiceObject;
        protected AndroidJavaObject viarusMarkerServiceObject;

        protected AndroidJavaObject cameraDeviceObject;
        protected AndroidJavaObject audioManager;

        public int HMDCameraId;
        public int ControllerCameraId;

        private bool isCameraPreviewing = false;

        private string systemDevice = "";

        public void Init()
        {
#if UNITY_ANDROID
            try
            {
                using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    androidActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
                    audioManager = androidActivity.Call<AndroidJavaObject>("getSystemService",
                        new AndroidJavaObject("java.lang.String", "audio"));
                }
            }
            catch (AndroidJavaException e)
            {
                androidActivity = null;
                Debug.LogError("Exception while connecting to the Activity: " + e);
                return;
            }

            viarusSDKClass = BaseAndroidDevice.GetClass(ViarusSDKClassName);

            
            

            viarusOsServiceObject = viarusSDKClass.CallStatic<AndroidJavaObject>("getNibiruOSService", androidActivity);
            viarusSensorServiceObject =
                viarusSDKClass.CallStatic<AndroidJavaObject>("getNibiruSensorService", androidActivity);
            viarusVoiceServiceObject =
                viarusSDKClass.CallStatic<AndroidJavaObject>("getNibiruVoiceService", androidActivity);
            viarusGestureServiceObject =
                viarusSDKClass.CallStatic<AndroidJavaObject>("getNibiruGestureService", androidActivity);
            viarusVRServiceObject = viarusSDKClass.CallStatic<AndroidJavaObject>("getUsingNibiruVRServiceGL");

            viarusCameraServiceObject =
                viarusSDKClass.CallStatic<AndroidJavaObject>("getNibiruCameraService", androidActivity);
            viarusMarkerServiceObject =
                viarusSDKClass.CallStatic<AndroidJavaObject>("getNibiruMarkerService", androidActivity);


            
            
            

            HMDCameraId = viarusCameraServiceObject.Call<int>("getHMDCameraId");
            
            Debug.LogWarning("2023mkbk ---getHMDCameraId -----== " + HMDCameraId);

            ControllerCameraId = viarusCameraServiceObject.Call<int>("getControllerCameraId");

            UpdateVoiceLanguage();
            ViarusTask.ViarusTaskApi.Init();

            IsCaptureEnabled = -1;

            
            RequsetPermission(new string[]
            {
                VrsGlobal.Permission.CAMERA,
                VrsGlobal.Permission.WRITE_EXTERNAL_STORAGE,
                VrsGlobal.Permission.READ_EXTERNAL_STORAGE,
                VrsGlobal.Permission.ACCESS_NETWORK_STATE,
                VrsGlobal.Permission.ACCESS_COARSE_LOCATION,
                VrsGlobal.Permission.BLUETOOTH,
                VrsGlobal.Permission.BLUETOOTH_ADMIN,
                VrsGlobal.Permission.INTERNET,
                VrsGlobal.Permission.GET_TASKS,
            });
#endif
        }

        public static int NKEY_SYS_HANDLE = 0;
        public static int NKEY_APP_HANDLE = 1;

        
        
        
        
        
        
        public void RegHandleNKey(int mode)
        {
            if (viarusVRServiceObject != null)
            {
                RunOnUIThread(androidActivity,
                    new AndroidJavaRunnable(() => { viarusVRServiceObject.Call("regHandleNKey", mode); }));
            }
            else
            {
                Debug.LogError("regHandleNKey failed, service is null !!!");
            }
        }

        
        
        
        
        public void SetEnableFPS(bool isEnabled)
        {
            if (viarusVRServiceObject != null)
            {
                viarusVRServiceObject.Call("setEnableFPS", isEnabled);
            }
            else
            {
                Debug.LogError("SetEnableFPS failed, service is null !!!");
            }
        }

        
        
        
        
        public float[] GetFPS()
        {
            if (viarusVRServiceObject != null)
            {
                return viarusVRServiceObject.Call<float[]>("getFPS");
            }
            else
            {
                Debug.LogError("SetEnableFPS failed, service is null !!!");
            }

            return new float[] {-1, -1};
        }

        
        
        
        
        public void RegisterVirtualMouseService(OnVirtualMouseServiceStatus serviceStatus)
        {
            if (viarusOsServiceObject != null)
            {
                viarusOsServiceObject.Call("registerVirtualMouseManagerService",
                    new ViarusVirtualMouseServiceListener(serviceStatus));
            }
            else
            {
                Debug.LogError("RegisterVirtualMouseService failed, service is null !!!");
            }
        }

        
        
        
        public void UnRegisterVirtualMouseService()
        {
            if (viarusOsServiceObject != null)
            {
                viarusOsServiceObject.Call("unRegisterVirtaulMouseManagerService");
            }
            else
            {
                Debug.LogError("UnRegisterVirtualMouseService failed, service is null !!!");
            }
        }

        
        
        
        
        
        public bool SetEnableVirtualMouse(bool enabled)
        {
            if (viarusOsServiceObject != null)
            {
                return viarusOsServiceObject.Call<bool>("setEnableVirtualMouse", enabled);
            }
            else
            {
                Debug.LogError("SetEnableVirtualMouse failed, service is null !!!");
                return false;
            }
        }

        
        
        
        public void GetCameraStatus()
        {
            int cameraId = HMDCameraId;
            Debug.LogWarning("2023mkbk cameraId == " + cameraId);
            if (viarusCameraServiceObject != null)
            {
                Debug.LogWarning("2023mkbk--start using java -- [getCameraStatus]");
                viarusCameraServiceObject.Call("getCameraStatus", cameraId, new CameraStatusCallback());
            }
            else
            {
                Debug.LogError("GetCameraStatus failed, because service is null !!!");
            }
        }

        
        
        
        
        private AndroidJavaObject OpenCamera()
        {
            int cameraId = HMDCameraId;
            if (viarusCameraServiceObject != null && cameraDeviceObject == null)
            {
                Debug.LogWarning("2023mkbk --start using java--[openCamera]");
                cameraDeviceObject = viarusCameraServiceObject.Call<AndroidJavaObject>("openCamera", cameraId);
                return cameraDeviceObject;
            }
            else if (cameraDeviceObject != null)
            {
                return cameraDeviceObject;
            }
            else
            {
                Debug.LogError("OpenCamera failed, because service is null !!!");
                return null;
            }
        }

        
        
        
        
        public CAMERA_ID GetCurrentCameraId()
        {
            return (CAMERA_ID) HMDCameraId;
        }

        
        
        
        public void StartCameraPreView()
        {
            StartCameraPreView(false);
        }

        
        
        
        
        public void StartCameraPreView(bool triggerFocus)
        {

            OpenCamera();
            isCameraPreviewing = true;
        }

        
        
        
        public void StopCamereaPreView()
        {
            if (viarusCameraServiceObject != null)
            {
                isCameraPreviewing = false;
                viarusCameraServiceObject.Call("stopPreview");
                cameraDeviceObject = null;
            }
            else
            {
                Debug.LogError("StopCamereaPreView failed, because service is null !!!");
            }
        }

        
        
        
        
        public bool IsCameraPreviewing()
        {
            return isCameraPreviewing;
        }

        public void SetCameraPreviewing(bool enabled)
        {
            isCameraPreviewing = true;
        }

        public void DoCameraAutoFocus()
        {
            if (cameraDeviceObject != null)
            {
                cameraDeviceObject.Call("doAutoFocus");
            }
            else
            {
                Debug.LogError("DoCameraAutoFocus failed, because service is null !!!");
            }
        }

        public void EnableVoiceService(bool enabled)
        {
            if (viarusVoiceServiceObject != null)
            {
                viarusVoiceServiceObject.Call("setEnableVoice", enabled);
            }
            else
            {
                Debug.LogError("EnableVoiceService failed, because service is null !!!");
            }
        }

        
        
        
        public void StartVoiceRecording()
        {
            if (viarusVoiceServiceObject != null)
            {
                viarusVoiceServiceObject.Call("startRecording");
            }
            else
            {
                Debug.LogError("StartVoiceRecording failed, because service is null !!!");
            }
        }

        
        
        
        public void StopVoiceRecording()
        {
            if (viarusVoiceServiceObject != null)
            {
                viarusVoiceServiceObject.Call("stopRecording");
            }
            else
            {
                Debug.LogError("StopVoiceRecording failed, because service is null !!!");
            }
        }

        
        
        
        public void CancelVoiceRecognizer()
        {
            if (viarusVoiceServiceObject != null)
            {
                viarusVoiceServiceObject.Call("cancelRecognizer");
            }
            else
            {
                Debug.LogError("CancelVoiceRecognizer failed, because service is null !!!");
            }
        }

        public bool IsSupportVoice()
        {
            if (viarusVoiceServiceObject != null)
            {
                return viarusVoiceServiceObject.Call<bool>("isMicrophoneVoice");
            }
            else
            {
                Debug.LogError("IsSupportVoice failed, because service is null !!!");
            }

            return false;
        }

        public bool IsSupport6DOF()
        {
            if (viarusVRServiceObject != null)
            {
                return viarusVRServiceObject.Call<bool>("isSupport6Dof");
            }
            else
            {
                Debug.LogError("IsSupport6DOF failed, because service is null !!!");
            }

            return false;
        }

        public bool IsSupportGesture()
        {
            if (viarusGestureServiceObject != null)
            {
                return viarusGestureServiceObject.Call<bool>("isCameraGesture");
            }
            else
            {
                Debug.LogError("isSupportGesture failed, because service is null !!!");
            }

            return false;
        }

        public void UpdateVoiceLanguage()
        {
            if (viarusVoiceServiceObject != null)
            {
                viarusVoiceServiceObject.Call("setVoicePID", (int) VrsGlobal.voiceLanguage);
            }
            else
            {
                Debug.LogError("UpdateVoiceLanguage failed, because service is null !!!");
            }
        }

        
        
        
        
        public void EnableGestureService(bool enabled)
        {
            if (viarusGestureServiceObject != null)
            {
                viarusGestureServiceObject.Call("setEnableGesture", enabled);
            }
            else
            {
                Debug.LogError("EnableGestureService failed, because service is null !!!");
            }
        }

        public bool IsCameraGesture()
        {
            if (viarusGestureServiceObject != null)
            {
                return viarusGestureServiceObject.Call<bool>("isCameraGesture");
            }

            return false;
        }

        public delegate void OnSensorDataChanged(ViarusSensorEvent sensorEvent);

        
        
        
        public static OnSensorDataChanged OnSensorDataChangedHandler;

        class ViarusSensorDataListenerCallback : AndroidJavaProxy
        {
            public ViarusSensorDataListenerCallback() : base(
                "com.nibiru.service.NibiruSensorService$INibiruSensorDataListener")
            {
            }

            public void onSensorDataChanged(AndroidJavaObject sensorEventObject)
            {
                float x = sensorEventObject.Get<float>("x");
                float y = sensorEventObject.Get<float>("y");
                float z = sensorEventObject.Get<float>("z");
                long timestamp = sensorEventObject.Get<long>("timestamp");
                AndroidJavaObject locationObject = sensorEventObject.Get<AndroidJavaObject>("sensorLocation");
                AndroidJavaObject typeObject = sensorEventObject.Get<AndroidJavaObject>("sensorType");
                SENSOR_LOCATION sensorLocation = (SENSOR_LOCATION) locationObject.Call<int>("ordinal");
                SENSOR_TYPE sensorType = (SENSOR_TYPE) typeObject.Call<int>("ordinal");

                ViarusSensorEvent sensorEvent = new ViarusSensorEvent(x, y, z, timestamp, sensorType, sensorLocation);
                

                
                Loom.QueueOnMainThread((param) =>
                {
                    if (OnSensorDataChangedHandler != null)
                    {
                        OnSensorDataChangedHandler((ViarusSensorEvent) param);
                    }
                }, sensorEvent);
            }
        }


        private ViarusSensorDataListenerCallback viarusSensorDataListenerCallback;

        public void RegisterSensorListener(SENSOR_TYPE type, SENSOR_LOCATION location)
        {
            if (viarusSensorServiceObject != null)
            {
                if (viarusSensorDataListenerCallback == null)
                {
                    viarusSensorDataListenerCallback = new ViarusSensorDataListenerCallback();
                }

                
                RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
                    {
                        AndroidJavaClass locationClass =
                            BaseAndroidDevice.GetClass("com.nibiru.service.NibiruSensorService$SENSOR_LOCATION");
                        AndroidJavaObject locationObj =
                            locationClass.CallStatic<AndroidJavaObject>("valueOf", location.ToString());

                        AndroidJavaClass typeClass =
                            BaseAndroidDevice.GetClass("com.nibiru.service.NibiruSensorService$SENSOR_TYPE");
                        AndroidJavaObject typeObj = typeClass.CallStatic<AndroidJavaObject>("valueOf", type.ToString());

                        viarusSensorServiceObject.Call<bool>("registerSensorListener", typeObj, locationObj,
                            viarusSensorDataListenerCallback);
                        Debug.Log("registerSensorListener=" + type.ToString() + "," + location.ToString());
                    }
                ));
            }
            else
            {
                Debug.LogError("RegisterControllerSensor failed, service is null !");
            }
        }

        public void UnRegisterSensorListener()
        {
            if (viarusSensorServiceObject != null)
            {
                
                RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
                    {
                        viarusSensorServiceObject.Call("unregisterSensorListenerAll");
                    }
                ));
            }
            else
            {
                Debug.LogError("UnRegisterSensorListener failed, service is null !");
            }
        }

        
        
        
        
        
        public int GetBrightnessValue()
        {
            int BrightnessValue = 0;
#if UNITY_ANDROID
            BaseAndroidDevice.CallObjectMethod<int>(ref BrightnessValue, viarusOsServiceObject, "getBrightnessValue");
#endif
            return BrightnessValue;
        }

        
        
        
        
        
        public void SetBrightnessValue(int value)
        {
            if (viarusOsServiceObject == null) return;
#if UNITY_ANDROID
            RunOnUIThread(androidActivity,
                new AndroidJavaRunnable(() =>
                {
                    BaseAndroidDevice.CallObjectMethod(viarusOsServiceObject, "setBrightnessValue", value, 200.01f);
                }));
#endif
        }

        
        
        
        
        
        public DISPLAY_MODE GetDisplayMode()
        {
            if (viarusOsServiceObject == null) return DISPLAY_MODE.MODE_2D;
            AndroidJavaObject androidObject = viarusOsServiceObject.Call<AndroidJavaObject>("getDisplayMode");
            int mode = androidObject.Call<int>("ordinal");
            return (DISPLAY_MODE) mode;
        }

        
        
        
        
        
        public void SetDisplayMode(DISPLAY_MODE displayMode)
        {
            if (viarusOsServiceObject != null)
            {
                RunOnUIThread(androidActivity,
                    new AndroidJavaRunnable(() =>
                    {
                        viarusOsServiceObject.Call("setDisplayMode", (int) displayMode);
                    }));
            }
        }

        
        
        
        
        
        public string GetChannelCode()
        {
            if (viarusOsServiceObject == null) return "NULL";
            return viarusOsServiceObject.Call<string>("getChannelCode");
        }

        
        
        
        
        
        public string GetModel()
        {
            if (viarusOsServiceObject == null) return "NULL";
            return viarusOsServiceObject.Call<string>("getModel");
        }

        
        
        
        
        
        public string GetOSVersion()
        {
            if (viarusOsServiceObject == null) return "NULL";
            return viarusOsServiceObject.Call<string>("getOSVersion");
        }

        
        
        
        
        
        public int GetOSVersionCode()
        {
            if (viarusOsServiceObject == null) return -1;
            return viarusOsServiceObject.Call<int>("getOSVersionCode");
        }

        
        
        
        
        
        public string GetServiceVersionCode()
        {
            if (viarusOsServiceObject == null) return "NULL";
            return viarusOsServiceObject.Call<string>("getServiceVersionCode");
        }

        
        
        
        
        
        public string GetVendorSWVersion()
        {
            if (viarusOsServiceObject == null) return "NULL";
            return viarusOsServiceObject.Call<string>("getVendorSWVersion");
        }

        
        
        
        
        
        public void SetEnableTouchCursor(bool isEnable)
        {
            RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
            {
                if (viarusOsServiceObject != null)
                {
                    viarusOsServiceObject.Call("setEnableTouchCursor", isEnable);
                }
            }));
        }

        
        
        
        
        public int GetProximityValue()
        {
            if (viarusSensorServiceObject == null) return -1;
            return viarusSensorServiceObject.Call<int>("getProximityValue");
        }

        
        
        
        
        public int GetLightValue()
        {
            if (viarusSensorServiceObject == null) return -1;
            return viarusSensorServiceObject.Call<int>("getLightValue");
        }

        
        public void RunOnUIThread(AndroidJavaObject activityObj, AndroidJavaRunnable r)
        {
            activityObj.Call("runOnUiThread", r);
        }

        public delegate void CameraIdle();

        public delegate void CameraBusy();

        
        
        
        public static CameraIdle OnCameraIdle;

        
        
        
        public static CameraBusy OnCameraBusy;

        public delegate void OnRecorderSuccess();

        public delegate void OnRecorderFailed();

        public static OnRecorderSuccess OnRecorderSuccessHandler;
        public static OnRecorderFailed OnRecorderFailedHandler;

        class CameraStatusCallback : AndroidJavaProxy
        {
            public CameraStatusCallback() : base("com.nibiru.lib.vr.listener.NVRCameraStatusListener")
            {
            }

            public void cameraBusy()
            {
                Debug.LogWarning("2023mkbk -- cameraBusy");
                
                
                Loom.QueueOnMainThread((param) =>
                {
                    if (OnCameraBusy != null)
                    {
                        OnCameraBusy();
                    }
                }, 1);
                Debug.Log("cameraBusy");
            }

            public void cameraIdle()
            {
                Debug.LogWarning("2023mkbk -- cameraIdle");
                
                
                Loom.QueueOnMainThread((param) =>
                {
                    if (OnCameraIdle != null)
                    {
                        Debug.LogWarning("2023mkbk -- start using OnCameraIdle();");
                        OnCameraIdle();
                    }
                }, 0);
                Debug.Log("cameraIdle");
            }
        }

        class CaptureCallback : AndroidJavaProxy
        {
            public CaptureCallback() : base("com.nibiru.lib.vr.listener.NVRVideoCaptureListener")
            {
            }

            public void onSuccess()
            {
                
                
                Loom.QueueOnMainThread((param) =>
                {
                    if (OnRecorderSuccessHandler != null)
                    {
                        OnRecorderSuccessHandler();
                    }
                }, 1);
            }

            public void onFailed()
            {
                
                
                Loom.QueueOnMainThread((param) =>
                {
                    if (OnRecorderFailedHandler != null)
                    {
                        OnRecorderFailedHandler();
                    }
                }, 0);
            }
        }

        public int IsCaptureEnabled { set; get; }

        public static int BIT_RATE = 4000000;

        
        
        
        
        public void StartCapture(string path)
        {
            StartCapture(path, -1);
        }

        
        
        
        
        
        public void StartCapture(string path, int seconds)
        {
            StartCapture(path, BIT_RATE, seconds);
        }

        private static int videoSize = (int) VIDEO_SIZE.V720P;

        
        
        
        
        
        
        
        public void StartCapture(string path, int bitRate, int seconds)
        {
            IsCaptureEnabled = 1;
            viarusSDKClass.CallStatic("startCaptureForUnity", new CaptureCallback(), path, bitRate, seconds, videoSize,
                HMDCameraId);
        }
        
        public enum CaptureCameraType{
            NORMAL,
            XVISIO
        }

        public AndroidJavaClass captureCameraTypeClass;
        public void SetCaptureCameraType(CaptureCameraType captureCameraType)
        {
            if (captureCameraTypeClass == null)
            {
                captureCameraTypeClass = new AndroidJavaClass("com.nibiru.lib.vr.video.CaptureCameraType");
            }

            var typeObj = captureCameraTypeClass.CallStatic<AndroidJavaObject>("valueOf",captureCameraType.ToString());
            viarusSDKClass.CallStatic("setCaptureCameraType",typeObj);
        }

        public static void SetCaptureVideoSize(VIDEO_SIZE video_Size)
        {
            videoSize = (int) video_Size;
        }

        
        
        
        public void StopCapture()
        {
            viarusSDKClass.CallStatic("stopCaptureForUnity");
            IsCaptureEnabled = 0;
        }

        public bool CaptureDrawFrame(int textureId, int frameId)
        {
            if (IsCaptureEnabled <= -3)
            {
                return false;
            }
            else if (IsCaptureEnabled <= 0 && IsCaptureEnabled >= -2)
            {
                
                IsCaptureEnabled--;
            }

            return viarusSDKClass.CallStatic<bool>("onDrawFrameForUnity", textureId, frameId);
        }

        private const int STREAM_VOICE_CALL = 0;
        private const int STREAM_SYSTEM = 1;
        private const int STREAM_RING = 2;
        private const int STREAM_MUSIC = 3;
        private const int STREAM_ALARM = 4;
        private const int STREAM_NOTIFICATION = 5;
        private const string currentVolume = "getStreamVolume"; 
        private const string maxVolume = "getStreamMaxVolume"; 

        public int GetVolumeValue()
        {
            if (audioManager == null) return 0;
            return audioManager.Call<int>(currentVolume, STREAM_MUSIC);
        }

        public int GetMaxVolume()
        {
            if (audioManager == null) return 1;
            return audioManager.Call<int>(maxVolume, STREAM_MUSIC);
        }

        public void EnabledMarkerAutoFocus(bool enabled)
        {
            if (viarusMarkerServiceObject == null)
            {
                Debug.LogError("service is null");
            }
            else if (isMarkerRecognizeRunning)
            {
                viarusMarkerServiceObject.Call(enabled ? "doAutoFocus" : "stopAutoFocus");
            }
        }

        
        
        
        
        private void SetMarkerRecognizeCameraId(int cameraID)
        {
            if (viarusMarkerServiceObject == null)
            {
                Debug.LogError("service is null");
            }
            else
            {
                viarusMarkerServiceObject.Call("setCameraId", cameraID);
            }
        }

        private bool isMarkerRecognizeRunning;

        public bool IsMarkerRecognizeRunning
        {
            get { return isMarkerRecognizeRunning; }
            set { isMarkerRecognizeRunning = value; }
        }

        
        
        
        public void StartMarkerRecognize()
        {
            if (viarusMarkerServiceObject == null)
            {
                Debug.LogError("service is null");
            }
            else if (!isMarkerRecognizeRunning)
            {
                
                SetMarkerRecognizeCameraId(HMDCameraId);
                
                viarusMarkerServiceObject.Call("setCameraZoom", VrsGlobal.GetMarkerCameraZoom());
                viarusMarkerServiceObject.Call("setPreviewSize", 640, 480);
                viarusMarkerServiceObject.Call("startMarkerRecognize");
                isMarkerRecognizeRunning = true;
            }
        }

        
        
        
        public void StopMarkerRecognize()
        {
            if (viarusMarkerServiceObject == null)
            {
                Debug.LogError("service is null");
            }
            else if (isMarkerRecognizeRunning)
            {
                viarusMarkerServiceObject.Call("stopMarkerRecognize");
                isMarkerRecognizeRunning = false;
            }
        }

        
        
        
        
        public float[] GetMarkerViewMatrix()
        {
            if (viarusMarkerServiceObject == null)
            {
                Debug.LogError("ss is null");
                return null;
            }
            else
            {
                float[] result = viarusMarkerServiceObject.Call<float[]>("getMarkerViewMatrix");
                if (result == null || result.Length == 0) return null;
                
                if (IsAllZero(result)) return null;
                return result;
            }
        }

        public static bool IsAllZero(float[] array)
        {
            for (int i = 0, l = array.Length; i < l; i++)
            {
                if (array[i] != 0) return false;
            }

            return true;
        }

        public float[] GetMarkerViewMatrix(int eyeType)
        {
            if (viarusMarkerServiceObject == null)
            {
                Debug.LogError("ss is null");
                return null;
            }
            else
            {
                float[] result = viarusMarkerServiceObject.Call<float[]>("getMarkerViewMatrix", eyeType);
                if (result == null || result.Length == 0) return null;
                
                if (IsAllZero(result)) return null;
                return result;
            }
        }

        public float[] GetMarkerProjectionMatrix()
        {
            if (viarusMarkerServiceObject == null)
            {
                Debug.LogError("service is null");
                return null;
            }
            else
            {
                float[] projArr = viarusMarkerServiceObject.Call<float[]>("getProjection");
                if (projArr == null || projArr.Length == 0)
                    return null;
                return projArr;
            }
        }

        public string GetMarkerDetectStatus()
        {
            if (viarusMarkerServiceObject == null)
            {
                Debug.LogError("GetMarkerDetectStatus failed, service is null");
                return "-1";
            }

            string res = viarusMarkerServiceObject.Call<string>("getParameters", "p_detect_status");
            return res == null ? "-1" : res;
        }

        public delegate void OnVirtualMouseServiceStatus(bool succ);

        public class ViarusVirtualMouseServiceListener : AndroidJavaProxy
        {
            OnVirtualMouseServiceStatus _OnVirtualMouseServiceStatus;

            public ViarusVirtualMouseServiceListener(OnVirtualMouseServiceStatus onVirtualMouseServiceStatus) : base(
                "com.nibiru.service.NibiruVirtualMouseManager$VirtualMouseServiceListener")
            {
                _OnVirtualMouseServiceStatus = onVirtualMouseServiceStatus;
            }

            public void onServiceRegisterResult(bool succ)
            {
                if (_OnVirtualMouseServiceStatus != null)
                {
                    _OnVirtualMouseServiceStatus(succ);
                }
            }
        }

        public void PauseGestureService()
        {
            if (viarusGestureServiceObject != null)
            {
                viarusGestureServiceObject.Call("onPause");
            }
            else
            {
                Debug.LogError("onPause failed, because service is null !!!");
            }
        }

        public void ResumeGestureService()
        {
            if (viarusGestureServiceObject != null)
            {
                viarusGestureServiceObject.Call("onResume");
            }
            else
            {
                Debug.LogError("onResume failed, because service is null !!!");
            }
        }

        private AndroidJavaObject javaArrayFromCS(string[] values)
        {
            AndroidJavaClass arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
            AndroidJavaObject arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance",
                new AndroidJavaClass("java.lang.String"), values.Count());
            for (int i = 0; i < values.Count(); ++i)
            {
                arrayClass.CallStatic("set", arrayObject, i, new AndroidJavaObject("java.lang.String", values[i]));
            }

            return arrayObject;
        }

        
        
        
        
        public void RequsetPermission(string[] names)
        {
            if (viarusOsServiceObject != null)
            {
                viarusOsServiceObject.Call("requestPermission", javaArrayFromCS(names));
            }
        }

        
        
        
        
        public QCOMProductDevice GetQCOMProductDevice()
        {
            if ("msm8996".Equals(systemDevice))
            {
                return QCOMProductDevice.QCOM_820;
            }
            else if ("msm8998".Equals(systemDevice))
            {
                return QCOMProductDevice.QCOM_835;
            }
            else if ("sdm710".Equals(systemDevice))
            {
                return QCOMProductDevice.QCOM_XR1;
            }
            else if ("sdm845".Equals(systemDevice))
            {
                return QCOMProductDevice.QCOM_845;
            }

            return QCOMProductDevice.QCOM_UNKNOW;
        }


        public void LockToCur()
        {
            if (viarusVRServiceObject != null)
            {
                RunOnUIThread(androidActivity,
                    new AndroidJavaRunnable(() => { viarusVRServiceObject.Call("lockTracker"); }));
            }
            else
            {
                Debug.LogError("LockToCur failed, service is null !!!");
            }
        }

        public void LockToFront()
        {
            if (viarusVRServiceObject != null)
            {
                RunOnUIThread(androidActivity,
                    new AndroidJavaRunnable(() => { viarusVRServiceObject.Call("lockTrackerToFront"); }));
            }
            else
            {
                Debug.LogError("LockToFront failed, service is null !!!");
            }
        }

        public void UnLock()
        {
            if (viarusVRServiceObject != null)
            {
                RunOnUIThread(androidActivity,
                    new AndroidJavaRunnable(() => { viarusVRServiceObject.Call("unlockTracker"); }));
            }
            else
            {
                Debug.LogError("UnLock failed, service is null !!!");
            }
        }


    }
}