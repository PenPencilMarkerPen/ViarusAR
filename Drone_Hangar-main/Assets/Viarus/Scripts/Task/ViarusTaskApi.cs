using Vrs.Internal;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ViarusTask
{
    
    
    
    
    public class ViarusTaskApi
    {
        public static string version = "V1.0.1_20180112";
        private const string ViarusSDKClassName = "com.nibiru.lib.vr.NibiruVR";

        private const int DeviceID = 1;

        protected static AndroidJavaObject androidActivity;
        protected static AndroidJavaClass viarusSDKClass;
        
        protected static AndroidJavaObject viarusOsServiceObject;

        
        public delegate void onSelectionResult(AndroidJavaObject task);

        public static onSelectionResult selectionCallback = null;   
        public static void setSelectionCallback(onSelectionResult callback)
        {
            selectionCallback = callback;
        }

        
        public delegate void onPowerChange(double task);

        public static onPowerChange powerChangeListener = null;   
        public static ViarusPowerListener viarusPowerListener;
        
        
        
        
        public static void addOnPowerChangeListener(onPowerChange listener)
        {
            if(listener != null)
            {
                powerChangeListener = listener;
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            viarusPowerListener = new ViarusPowerListener();
#endif
            if (viarusOsServiceObject != null)
            {
                viarusOsServiceObject.Call("registerPowerChangeListener", viarusPowerListener);
            }
        }

        
        
        
        
        public static void removeOnPowerChangeListener(onPowerChange listener)
        {
            powerChangeListener = null;
            if (viarusOsServiceObject != null && viarusPowerListener != null)
            {
                viarusOsServiceObject.Call("unregisterPowerChangeListener", viarusPowerListener);
            }
        }

        
        public delegate void onServerApiReady(bool isReady);
        public static onServerApiReady serverApiReady = null;   
        public static IServerApiReadyListener serverApiReadyListener;
        public static void addOnServerApiReadyCallback(onServerApiReady callback)
        {
            serverApiReady = callback;
            
            
            
            
            
            
            
        }
        
        public static void removeOnServerApiReadyCallback(onServerApiReady callback)
        {
            serverApiReady = null;
            
        }

        
        public delegate void onSysSleepApiReady(bool isReady);

        public static onSysSleepApiReady sysSleepApiReady = null;   
        public static ISysSleepApiReadyListener sysSleepApiReadyListener;
        public static void addOnSysSleepApiReadyCallback(onSysSleepApiReady callback)
        {
            sysSleepApiReady = callback;

            
            
            
            
            
            
        }
        public static void removeOnSysSleepApiReadyCallback(onSysSleepApiReady callback)
        {
            sysSleepApiReady = null;
            
        }

        public delegate void onDeviceConnectState(int state, CDevice device);

        public static onDeviceConnectState deviceConnectState = null;   
        public static void setOnDeviceListener(onDeviceConnectState listener)
        {
            deviceConnectState = listener;
            ControllerAndroid.setOnDeviceListener(new OnDeviceListener());
        }

        public static void Init()
        {
            if (viarusOsServiceObject != null) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.Log("-------ViarusTaskLib-------Version-------" + version);
            ConnectToActivity();
            viarusSDKClass = GetClass(ViarusSDKClassName);
            viarusOsServiceObject = viarusSDKClass.CallStatic<AndroidJavaObject>("getNibiruOSService", androidActivity);

            ControllerAndroid.onStart();
#endif
        }

        
        
        
        
        
        public static bool IsPluginDeclared(PLUGIN_ID id)
        {
            if (viarusSDKClass == null)
            {
                Init();
            }

            if (viarusSDKClass == null) return false;
            return viarusSDKClass.CallStatic<bool>("isPluginDeclared", (int)id);
        }

        
        
        
        
        
        public static bool IsPluginSupported(PLUGIN_ID id)
        {
            if (viarusSDKClass == null)
            {
                Init();
            }
            if (viarusSDKClass == null) return false;
            return viarusSDKClass.CallStatic<bool>("isPluginSupported", (int)id);
        }

        
        
        
        
        
        public static bool LaunchAppByPkgName(string pkgName)
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject != null)
            {
                return viarusOsServiceObject.Call<bool>("launchAppByPkgName", pkgName);
            }

            return false;
        }

        
        
        
        
        public static string GetDeviceId()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject != null)
            {
                return viarusOsServiceObject.Call<string>("getDeviceId");
            }
            return "null";
        }

        
        
        
        
        public static string GetMacAddress()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject != null)
            {
                return viarusOsServiceObject.Call<string>("getMacAddress");
            }
            return "null";
        }

        
        
        
        
        public static int GetBluetoothStatus()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusSDKClass != null)
            {
                return viarusSDKClass.CallStatic<int>("getBluetoothStatus");
            }
            return 0;
        }

        
        
        
        
        public static int GetNetworkStatus()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusSDKClass != null)
            {
                return viarusSDKClass.CallStatic<int>("getNetworkStatus");
            }
            return 0;
        }

        public static void Destroy()
        {
            ControllerAndroid.onStop();
        }

        
        
        
        public static void OpenDeviceDriver()
        {
            AndroidJavaObject task = ViarusTaskApi.GetViarusTask(TASK_ACTION.DEVICE_DRIVER);
            StartViarusTask(task);
        }
        

        
        public static void OpenSettingsWifi()
        {
            AndroidJavaObject task = ViarusTaskApi.GetViarusTask(TASK_ACTION.SETTINGS);
            AddViarusTaskParameter(task, Setting.SETTINGS_KEY_TYPE, Setting.SETTINGS_TYPE_WIFI);
            StartViarusTask(task);
        }

        
        
        
        
        public static void OpenSettingsBluetooth()
        {
            AndroidJavaObject task = ViarusTaskApi.GetViarusTask(TASK_ACTION.SETTINGS);
            AddViarusTaskParameter(task, Setting.SETTINGS_KEY_TYPE, Setting.SETTINGS_TYPE_BLUETOOTH);
            StartViarusTask(task);
        }

        
        
        
        
        public static void OpenSettingsSystem()
        {
            AndroidJavaObject task = ViarusTaskApi.GetViarusTask(TASK_ACTION.SETTINGS);
            AddViarusTaskParameter(task, Setting.SETTINGS_KEY_TYPE, Setting.SETTINGS_TYPE_SYSTEM);
            StartViarusTask(task);
        }

        
        
        
        
        public static void OpenSettingsGeneral()
        {
            AndroidJavaObject task = ViarusTaskApi.GetViarusTask(TASK_ACTION.SETTINGS);
            AddViarusTaskParameter(task, Setting.SETTINGS_KEY_TYPE, Setting.SETTINGS_TYPE_GENERAL);
            StartViarusTask(task);
        }

        
        
        
        
        public static void OpenSettingsMain()
        {
            AndroidJavaObject task = ViarusTaskApi.GetViarusTask(TASK_ACTION.SETTINGS);
            AddViarusTaskParameter(task, Setting.SETTINGS_KEY_TYPE, Setting.SETTINGS_TYPE_MAIN);
            StartViarusTask(task);
        }

        
        public static void StartViarusTask(AndroidJavaObject task)
        {
            if (task == null) return;
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            
            RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
            {
                viarusOsServiceObject.Call<bool>("startNibiruTask", task);
            }
            ));
        }

        public static AndroidJavaObject GetViarusTask(TASK_ACTION action)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Create("com.nibiru.service.NibiruTask", (int)action);
#else
            return null;
#endif
        }

        public static AndroidJavaObject GetViarusSelectionTask(SELECTION_TASK_ACTION action)
        {
            AndroidJavaObject task = Create("com.nibiru.service.NibiruSelectionTask", (int)action);
            task.Call("setCallback", new ViarusSelectionCallback());
            return task;
        }

        public static void AddViarusTaskParameter(AndroidJavaObject task, string key, string value)
        {
            if (task == null) return;
            task.Call("addParameter", key, value);
        }

        public static void AddViarusTaskParameter(AndroidJavaObject task, string key, int value)
        {
            if (task == null) return;
            task.Call("addParameter", key, value);
        }

        public static void AddViarusTaskParameter(AndroidJavaObject task, string key, long value)
        {
            if (task == null) return;
            task.Call("addParameter", key, value);
        }
        
        
        public static string GetChannelCode()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return "null";
            }
            return viarusOsServiceObject == null ? "null" : viarusOsServiceObject.Call<string>("getChannelCode");
        }

        
        
        
        
        public static string GetModel()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return "null";
            }
            return viarusOsServiceObject == null ? "null" : viarusOsServiceObject.Call<string>("getModel");
        }

        
        
        
        
        public static string GetOSVersion()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return "null";
            }
            return viarusOsServiceObject == null ? "null" : viarusOsServiceObject.Call<string>("getOSVersion");
        }

        
        
        
        
        public static int GetOSVersionCode()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return -1;
            }
            return viarusOsServiceObject == null ? -1 : viarusOsServiceObject.Call<int>("getOSVersionCode");
        }
 
        
        
        
        
        public static string GetServiceVersionCode()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }

            if (viarusOsServiceObject == null)
            {
                return "null";
            }
            return viarusOsServiceObject == null ? "null" : viarusOsServiceObject.Call<string>("getServiceVersionCode");
        }

        
        
        
        
        public static string GetVendorSWVersion()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return "null";
            }
            return viarusOsServiceObject == null ? "null" : viarusOsServiceObject.Call<string>("getVendorSWVersion");
        }
    
        
        
        
        
        public static int GetBrightnessValue()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return 0;
            }
            return viarusOsServiceObject == null ? 0 : viarusOsServiceObject.Call<int>("getBrightnessValue");
        }

        
        
        
        
        public static void SetBrightnessValue(int value)
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            viarusOsServiceObject.Call("setBrightnessValue", value);
        }

        
        public static DISPLAY_MODE GetDisplayMode()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null) return DISPLAY_MODE.MODE_2D;
            AndroidJavaObject androidObject = viarusOsServiceObject.Call<AndroidJavaObject>("getDisplayMode");
            int mode = androidObject.Call<int>("ordinal");
            return (DISPLAY_MODE)mode;
        }

        
        
        
        
        
        public static void SetDisplayMode(DISPLAY_MODE displayMode)
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject != null)
            {
                RunOnUIThread(androidActivity, new AndroidJavaRunnable(() =>
                {
                    viarusOsServiceObject.Call("setDisplayMode", (int)displayMode);
                }));
            }
        }

        
        
        
        
        public static int GetSysSleepTime()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return 0;
            }
            return viarusOsServiceObject.Call<int>("getSysSleepTime");
        }

        
        
        
        
        public static string GetCurrentTimezone()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if(viarusOsServiceObject == null)
            {
                return "null";
            }
            return viarusOsServiceObject.Call<string>("getCurrentTimezone");
        }

        
        
        
        
        public static List<string> GetLanguageList()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            List<string> languageList = new List<string>();
            AndroidJavaObject languages = viarusOsServiceObject.Call<AndroidJavaObject>("getLanguageList");
            if (languages != null)
            {
                int size = languages.Call<int>("size");
                for (int i = 0; i < size; i++)
                {
                    languageList.Add(languages.Call<string>("get", i));
                }
            }
            return languageList;

        }

        
        
        
        
        public static string GetCurrentLanguage()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }

            if (viarusOsServiceObject == null) return "null";
            return viarusOsServiceObject.Call<string>("getCurrentLanguage");
        }

        
        
        
        
        public static string GetVRVersion()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return "null";
            }
            return viarusOsServiceObject.Call<string>("getVRVersion");
        }

        
        
        
        
        public static string GetDeviceName()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            if (viarusOsServiceObject == null)
            {
                return "null";
            }
            return viarusOsServiceObject.Call<string>("getDeviceName");
        }

        
        
        
        public static void OpenWps()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            viarusOsServiceObject.Call("openWps");
        }

        public static void SetWifiEnable(bool enable)
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }

            if (viarusOsServiceObject != null)
            {
                RunOnUIThread(androidActivity, () =>
                {
                    viarusOsServiceObject.Call("setWifiEnable", enable); 
                });
            }
        }

        public static bool IsWifiEnabled()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }
            
            if (viarusOsServiceObject != null)
            {
                return viarusOsServiceObject.Call<bool>("isWifiEnabled");
            }

            return false;
        }

        public static void SetBluetoothEnable(bool enable)
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }

            if (viarusOsServiceObject != null)
            {
                RunOnUIThread(androidActivity,(() =>
                {
                    viarusOsServiceObject.Call("setBluetoothEnable", enable);
                }));
            }
        }

        public static bool IsBluetoothEnabled()
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }

            if (viarusOsServiceObject != null)
            {
                return viarusOsServiceObject.Call<bool>("isBluetoothEnabled");
            }

            return false;
        }
        
        public static void GoToControllerDriver()
        {
            LaunchAppByPkgName("com.nibiru.vrdriver");
        }

        public static void InstallApk(string apkPath)
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }

            if (viarusOsServiceObject != null)
            {
                viarusOsServiceObject.Call("installApk", apkPath, new ViarusApkInstallListener());
            }
        }

        public static void UninstallApk(string packageName)
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }

            if (viarusOsServiceObject != null)
            {
                viarusOsServiceObject.Call("uninstallApk", packageName, new ViarusApkInstallListener());
            }
        }

        public delegate void InstallSuccessCallback(string filePath, string packageName);

        public delegate void InstallFailedCallback(string filePath, string packageName);

        public delegate void UninstallSuccessCallback(string packageName);

        public delegate void UninstallFailedCallback(string packageName);

        public static InstallSuccessCallback InstallSuccessEvent;
        public static InstallFailedCallback InstallFailedEvent;
        public static UninstallSuccessCallback UninstallSuccessEvent;
        public static UninstallFailedCallback UninstallFailedEvent;

        public static void SetInstallSuccessCallback(InstallSuccessCallback callback)
        {
            if (callback != null)
            {
                InstallSuccessEvent = callback;
            }
        }
        
        public static void SetInstallFailedCallback(InstallFailedCallback callback)
        {
            if (callback != null)
            {
                InstallFailedEvent = callback;
            }
        }
        
        public static void SetUninstallSuccessCallback(UninstallSuccessCallback callback)
        {
            if (callback != null)
            {
                UninstallSuccessEvent = callback;
            }
        }
        
        public static void SetUninstallFailedCallback(UninstallFailedCallback callback)
        {
            if (callback != null)
            {
                UninstallFailedEvent = callback;
            }
        }
        
        public static void InstallAndStartApk(string apkPath)
        {
            if (viarusOsServiceObject == null)
            {
                Init();
            }

            if (viarusOsServiceObject != null)
            {
                viarusOsServiceObject.Call("installAndStartApk", apkPath);
            }
        }
        
        
        
        
        
        
        

        public static int[] GetKeyAction(int deviceId = DeviceID)
        {
            return ControllerAndroid.getKeyAction(deviceId);
        }

        
        public static List<string> getCSupportDevices()
        {
            List<string> deviceList = new List<string>();
            AndroidJavaObject devices = ControllerAndroid.getCSupportDevices();
            if (devices != null)
            {
                int size = devices.Call<int>("size");
                for (int i = 0; i < size; i++)
                {
                    deviceList.Add(devices.Call<string>("get", i));
                }
            }
            return deviceList;
        }
        
        public static CSensorEvent GetCSensorEvent(int type, int deviceId = DeviceID)
        {
            AndroidJavaObject sensorEvent = ControllerAndroid.getCSensorEvent(type, deviceId);
            if (sensorEvent == null)
                return null;
            return new CSensorEvent(sensorEvent.Call<int>("getType"), sensorEvent.Call<int>("getDeviceId"), sensorEvent.Call<long>("getEventTime"), sensorEvent.Call<float[]>("getValues"));
        }
        
        public static void Connect(AndroidJavaObject bluetoothDevice)
        {
            ControllerAndroid.connect(bluetoothDevice);
        }
        
        public static void Disconnect(AndroidJavaObject bluetoothDevice)
        {
            ControllerAndroid.disconnect(bluetoothDevice);
        }
        
        public static List<CDevice> GetCDevices()
        {
            List<CDevice> deviceList = new List<CDevice>();
            AndroidJavaObject devices = ControllerAndroid.getCDevices();
            if (devices != null)
            {
                int size = devices.Call<int>("size");
                for (int i = 0; i < size; i++)
                {
                    AndroidJavaObject device = devices.Call<AndroidJavaObject>("get", i);
                    if (device != null)
                    {
                        AndroidJavaObject usbDevice = device.Call<AndroidJavaObject>("getUdevice");
                        AndroidJavaObject bluetoothDevice = device.Call<AndroidJavaObject>("getBdevice");
                        if (usbDevice != null)
                        {
                            deviceList.Add(new CDevice(usbDevice, device.Call<bool>("isQuat"), device.Call<int>("getType")));
                        }
                        else if (bluetoothDevice != null)
                        {
                            deviceList.Add(new CDevice(bluetoothDevice, device.Call<bool>("isQuat"), device.Call<int>("getType"), device.Call<int>("getMode")));
                        }
                        else
                        {
                            deviceList.Add(new CDevice(device.Call<string>("getName"), device.Call<bool>("isQuat"), device.Call<int>("getType"), device.Call<int>("getMode")));
                        }
                    }
                }
            }
            return deviceList;
        }
        
        public static int GetHandMode()
        {
            return ControllerAndroid.getHandMode();
        }

        
        
        
        
        public static float[] GetMotion(int deviceId = DeviceID)
        {
            return ControllerAndroid.getMotion(deviceId);
        }
        
        
        public static float[] GetQuat(int deviceId = DeviceID)
        {
            return ControllerAndroid.getQuat(deviceId);
        }
        
        
        
        
        
        
        
        public static int[] GetBrain(int deviceId = DeviceID)
        {
            return ControllerAndroid.getBrain(deviceId);
        }
        
        public static int GetBlink(int deviceId = DeviceID)
        {
            return ControllerAndroid.getBlink(deviceId);
        }
        
        public static int GetGesture(int deviceId = DeviceID)
        {
            return ControllerAndroid.getGesture(deviceId);
        }
        
        public static float[] GetTouch(int deviceId = DeviceID)
        {
            return ControllerAndroid.getTouch(deviceId);
        }
        
        public static int[] GetDeviceIds()
        {
            return ControllerAndroid.getDeviceIds();
        }
        
        public static bool IsQuatConn()
        {
            return ControllerAndroid.isQuatConn();
        }
        
        public static int GetDeviceType()
        {
            return ControllerAndroid.getDeviceType();
        }

        
        public static void SetMotionType(int motionType)
        {
            ControllerAndroid.setMotionType(motionType);
        }

        
        
        

        protected static void ConnectToActivity()
        {
            try
            {
                using (AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    androidActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
                }
            }
            catch (AndroidJavaException e)
            {
                androidActivity = null;
                Debug.LogError("Exception while connecting to the Activity: " + e);
            }
        }

        public static AndroidJavaClass GetClass(string className)
        {
            try
            {
                return new AndroidJavaClass(className);
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception getting class " + className + ": " + e);
                return null;
            }
        }

        public static AndroidJavaObject Create(string className, params object[] args)
        {
            try
            {
                return new AndroidJavaObject(className, args);
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception creating object " + className + ": " + e);
                return null;
            }
        }

        public static bool CallStaticMethod(AndroidJavaObject jo, string name, params object[] args)
        {
            if (jo == null)
            {
                Debug.LogError("Object is null when calling static method " + name);
                return false;
            }
            try
            {
                jo.CallStatic(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling static method " + name + ": " + e);
                return false;
            }
        }

        public static bool CallObjectMethod(AndroidJavaObject jo, string name, params object[] args)
        {
            if (jo == null)
            {
                Debug.LogError("Object is null when calling method " + name);
                return false;
            }
            try
            {
                jo.Call(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling method " + name + ": " + e);
                return false;
            }
        }

        public static bool CallStaticMethod<T>(ref T result, AndroidJavaObject jo, string name,
                                                  params object[] args)
        {
            if (jo == null)
            {
                Debug.LogError("Object is null when calling static method " + name);
                return false;
            }
            try
            {
                result = jo.CallStatic<T>(name, args);
                return true;
            }
            catch (AndroidJavaException e)
            {
                Debug.LogError("Exception calling static method " + name + ": " + e);
                return false;
            }
        }

        
        public static void RunOnUIThread(AndroidJavaObject activityObj, AndroidJavaRunnable r)
        {
            activityObj.Call("runOnUiThread", r);
        }

    }

    
    
    
    public class ViarusSelectionCallback : AndroidJavaProxy
    {
        
        public ViarusSelectionCallback() : base("com.nibiru.service.NibiruSelectionTask$NibiruSelectionCallback")
        {

        }

        public void onSelectionResult(AndroidJavaObject task)
        {
            Debug.Log("onSelectionResult");
            if (ViarusTaskApi.selectionCallback != null)
            {
                ViarusTaskApi.selectionCallback(task);
            }
            
        }

    }

    public class ViarusPowerListener : AndroidJavaProxy
    {
        public ViarusPowerListener() : base("com.nibiru.service.NibiruOSService$INibiruPowerChangeListener")
        {

        }

        public void onPowerChanged(double power)
        {
            Debug.Log("onPowerChange:" + power);
            if (ViarusTaskApi.powerChangeListener != null)
            {
                ViarusTaskApi.powerChangeListener(power);
            }
        }

#if UNITY_2017_4_OR_NEWER
#else
        public bool equals(AndroidJavaObject obj)
        {
            Debug.Log("onEqual:" + base.Equals(obj));
            
            return true;
        }
#endif
    }

    public class IServerApiReadyListener : AndroidJavaProxy
    {
        public IServerApiReadyListener() : base("com.nibiru.service.NibiruOSService$IServerApiReadyListener")
        {

        }

        public void onServerApiReady(bool isReady)
        {
            Debug.Log("onServerApiReady:" + isReady);
            if (ViarusTaskApi.serverApiReady != null)
            {
                ViarusTaskApi.serverApiReady(isReady);
            }
        }

    }

    public class ISysSleepApiReadyListener : AndroidJavaProxy
    {
        public ISysSleepApiReadyListener() : base("com.nibiru.service.NibiruOSService$ISysSleepApiReadyListener")
        {

        }

        public void onSysSleepApiReady(bool isReady)
        {
            Debug.Log("onSysSleepApiReady:" + isReady);
            if (ViarusTaskApi.sysSleepApiReady != null)
            {
                ViarusTaskApi.sysSleepApiReady(isReady);
            }
        }

    }

    
    public class OnDeviceListener : AndroidJavaProxy
    {
        public OnDeviceListener() : base("ruiyue.controller.sdk.IIControllerService$OnDeviceListener")
        {

        }

        public void onDeviceConnectState(int state, AndroidJavaObject device)
        {
            Debug.Log("ViarusTaskApi.onDeviceConnectState:" + state + "(0=connect,1=disconnect)");
            if (!InteractionManager.IsInteractionSDKEnabled())
            {
                VrsSDKApi.Instance.ExecuteControllerStatusChangeEvent(InteractionManager.NACTION_HAND_TYPE.HAND_RIGHT, state == 0, false);
            }
            CDevice cDevice = null;
            if (device != null)
            {
                IntPtr usbDevicePtr = device.Call<IntPtr>("getUdevice");
                IntPtr bluetoothDevicePtr = device.Call<IntPtr>("getBdevice");
                AndroidJavaObject usbDevice = usbDevicePtr == IntPtr.Zero ? null : device.Call<AndroidJavaObject>("getUdevice");
                AndroidJavaObject bluetoothDevice = bluetoothDevicePtr == IntPtr.Zero ? null : device.Call<AndroidJavaObject>("getBdevice");
                if (usbDevice != null)
                {
                    cDevice = new CDevice(usbDevice, device.Call<bool>("isQuat"), device.Call<int>("getType"));
                }
                else if (bluetoothDevice != null)
                {
                    cDevice = new CDevice(bluetoothDevice, device.Call<bool>("isQuat"), device.Call<int>("getType"), device.Call<int>("getMode"));
                }
                else
                {
                    cDevice = new CDevice(device.Call<string>("getName"), device.Call<bool>("isQuat"), device.Call<int>("getType"), device.Call<int>("getMode"));
                }
            }
            if (ViarusTaskApi.deviceConnectState != null)
            {
                ViarusTaskApi.deviceConnectState(state, cDevice);
            }
        }
    }

    public class ViarusApkInstallListener : AndroidJavaProxy
    {
        public ViarusApkInstallListener() : base("com.nibiru.service.NibiruOSService$INibiruApkInstallListener")
        {
        }

        public void onInstallSuccess(string filePath, string packageName)
        {
            Debug.Log( packageName+"PackageInstallSuccess!");
            Debug.Log("mkbk_install" + packageName + "PackageInstallSuccess!");
            if (ViarusTaskApi.InstallSuccessEvent != null)
            {
                ViarusTaskApi.InstallSuccessEvent(filePath, packageName);
            }
        }

        public void onInstallFailed(string filePath, string packageName)
        {
            Debug.Log( packageName+"PackageInstallFail!");
            Debug.Log("mkbk_install"+packageName + "PackageInstallFail!");
            if (ViarusTaskApi.InstallFailedEvent != null)
            {
                ViarusTaskApi.InstallFailedEvent(filePath, packageName);
            }
        }
        
        public void onUninstallFailed(string packageName)
        {
            Debug.Log( packageName+"PackageUninstallFail!");
            Debug.Log("mkbk_install" + packageName + "PackageUninstallFail!");
            if (ViarusTaskApi.UninstallFailedEvent != null)
            {
                ViarusTaskApi.UninstallFailedEvent(packageName);
            }
        }

        public void onUninstallSuccess(string packageName)
        {
            Debug.Log( packageName+"PackageUninstallSuccess!");
            Debug.Log("mkbk_install" + packageName + "PackageUninstallSuccess!");
            if (ViarusTaskApi.UninstallSuccessEvent != null)
            {
                ViarusTaskApi.UninstallSuccessEvent(packageName);
            }
        }
    }
}
