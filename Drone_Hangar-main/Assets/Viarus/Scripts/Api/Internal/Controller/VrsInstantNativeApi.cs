using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Vrs.Internal
{
    public class VrsInstantNativeApi
    {
        #region Struct
        [StructLayout(LayoutKind.Sequential)]
        public struct Viarus_Pose
        {
            public Vector3 position;
            public Quaternion rotation;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct Viarus_ControllerStates
        {
            public uint battery; 
            public uint connectStatus;
            public uint buttons;
            public uint hmdButtons;
            public uint touches;
            public Vector2 touchpadAxis;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Viarus_Config
        {
            public uint controllerType; 
            public float ipd;
            public float near;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public float[] eyeFrustumParams;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public uint[] textureSize; 

        }

        public struct NvrInitParams
        {
            public int renderWidth;
            public int renderHeight;
            public int bitRate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DebugInfo
        {
            public int frameIndex;
        }
        #endregion

        public enum ViarusDeviceType
        {
            Hmd = 0,
            LeftController,
            RightController,
            None=3
        }

        
        public enum ViarusControllerId
        {
            
            NOLO, EXPAND, NORMAL_3DOF, NONE
        }

        public enum RenderEvent
        {
            SubmitFrame = 1
        };

        public static bool Inited = false;
        public static int nativeApiVersion = -1;
        public static int driverVersion = -1;

#if UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
        internal const string dllName = "NvrPluginNative";

        [DllImport(dllName)]
        public static extern void SetVersionInfo(int apiVersion, string unity_version_str, int unity_version_length);

        [DllImport(dllName)]
        public static extern bool Init(NvrInitParams args);

        [DllImport(dllName)]
        public static extern IntPtr GetRenderEventFunc();

        [DllImport(dllName)]
        public static extern void SetFrameTexture(IntPtr texturePointer);

        [DllImport(dllName)]
        public static extern void Cleanup();

        [DllImport(dllName)]
        public static extern Viarus_Config GetViarusConfig();

        
        [DllImport(dllName)]
        public static extern Viarus_Pose GetPoseByDeviceType(ViarusDeviceType type);

        [DllImport(dllName)]
        public static extern Viarus_ControllerStates GetControllerStates(ViarusDeviceType type);

        [DllImport(dllName)]
        public static extern void SetViarusConfigCallback(VrsViewer.ViarusConfigCallback callback);

        
        [DllImport(dllName)]
        public static extern void GetVersionInfo(ref int apiVersion, ref int driverVersion);

        [DllImport(dllName)]
        public static extern void SendFrame();

        [DllImport(dllName)]
        public static extern void GetTextureResolution(ref int width, ref int height);

        [DllImport(dllName)]
        public static extern DebugInfo GetDebugInfo();

        


        [DllImport(dllName)]
        public static extern UInt32 GetDecodeRate();

        [DllImport(dllName)]
        public static extern UInt32 GetRefreshRate();

        [DllImport(dllName)]
        public static extern IntPtr GetLeapMotionData();
        
        

#elif UNITY_ANDROID

        public static void SetVersionInfo(int apiVersion, string unity_version_str, int unity_version_length) { }

        public static bool Init(NvrInitParams args) { return false; }

        public static IntPtr GetRenderEventFunc() { return IntPtr.Zero; }

        public static void SetFrameTexture(IntPtr texturePointer) { }

        public static void Cleanup() { }

        public static Viarus_Config GetViarusConfig() { return new Viarus_Config(); }

        
        public static Viarus_Pose GetPoseByDeviceType(ViarusDeviceType type) { return new Viarus_Pose(); }

        public static Viarus_ControllerStates GetControllerStates(ViarusDeviceType type) { return new Viarus_ControllerStates(); }

        public static void SetViarusConfigCallback(VrsViewer.ViarusConfigCallback callback) { }

        
        public static void GetVersionInfo(ref int apiVersion, ref int driverVersion) { }

        public static void SendFrame() { }

        public static void GetTextureResolution(ref int width, ref int height) { }

        public static DebugInfo GetDebugInfo() { return new DebugInfo(); }
        
        public static IntPtr GetLeapMotionData() { return IntPtr.Zero; }
        public static UInt32 GetRefreshRate() { return 0; }
        public static UInt32 GetDecodeRate() { return 0; }
        
        
#endif

    }
}
