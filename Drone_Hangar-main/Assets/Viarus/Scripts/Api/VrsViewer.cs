













using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using ViarusTask;
using ViarusAxis;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Vrs.Internal
{
    
    
    
    [AddComponentMenu("VRS/VrsViewer")]
    public class VrsViewer : MonoBehaviour
    {
        
        public const string VRS_SDK_VERSION = "2.5.3.0_20230109";

        
        public const bool IsAndroidKillProcess = true;

        
        public static bool USE_DTR = true;

        private static int _texture_count = 6;

        
        public static int kPreRenderEvent = 1;

        
        private float[] headEulerAnglesRange = null;

        
        public UnityAction OnUpdateActionHandler;
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("OnSceneLoaded->" + scene.name + " , Triggered=" + Triggered);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        
        public static VrsViewer Instance
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
                USE_DTR = false;
                if (instance == null && !Application.isPlaying)
                {
                    Debug.Log("Create VrsViewer Instance !");
                    instance = FindObjectOfType<VrsViewer>();
                }
#endif
                if (instance == null)
                {
                    Debug.LogError("No VrsViewer instance found.  Ensure one exists in the scene, or call "
                                   + "VrsViewer.Create() at startup to generate one.\n"
                                   + "If one does exist but hasn't called Awake() yet, "
                                   + "then this error is due to order-of-initialization.\n"
                                   + "In that case, consider moving "
                                   + "your first reference to VrsViewer.Instance to a later point in time.\n"
                                   + "If exiting the scene, this indicates that the VrsViewer object has already "
                                   + "been destroyed.");
                }

                return instance;
            }
        }

        private static VrsViewer instance = null;
        public VrsEye[] eyes = new VrsEye[2];

        private byte[] winTypeName = new byte[]
            {110, 120, 114, 46, 78, 118, 114, 87, 105, 110, 66, 97, 115, 101}; 

        
        public static void Create()
        {
            if (instance == null && FindObjectOfType<VrsViewer>() == null)
            {
                Debug.Log("Creating VrsViewerMain object");
                var go = new GameObject("VrsViewerMain", typeof(VrsViewer));
                go.transform.localPosition = Vector3.zero;
                
            }
        }

        
        
        public VrsStereoController Controller
        {
            get
            {
                if (currentController == null)
                {
                    currentController = FindObjectOfType<VrsStereoController>();
                }

                return currentController;
            }
        }

        private VrsStereoController currentController;

        [SerializeField] public HMD_TYPE HmdType = HMD_TYPE.NONE;

        
        
        
        
        [SerializeField] private bool openEffectRender = false;

        
        
        
        public bool OpenEffectRender
        {
            get { return openEffectRender; }
            set
            {
                if (value != openEffectRender)
                {
                    openEffectRender = value;
                }
            }
        }

        
        
        public bool SplitScreenModeEnabled
        {
            get { return splitScreenModeEnabled; }
            set
            {
                if (value != splitScreenModeEnabled && device != null)
                {
                    device.SetSplitScreenModeEnabled(value);
                }

                splitScreenModeEnabled = value;
            }
        }

        [SerializeField] private bool splitScreenModeEnabled = true;


        
        
        
        public HeadControl HeadControl
        {
            get { return headControlEnabled; }
            set
            {
                headControlEnabled = value;
                UpdateHeadControl();
            }
        }

        [SerializeField] private HeadControl headControlEnabled = HeadControl.GazeApplication;


        public float Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        [SerializeField] private float duration = 2;

        VrsReticle mVrsReticle;

        public VrsReticle GetVrsReticle()
        {
            InitVrsReticleScript();
            return mVrsReticle;
        }

        public void DismissReticle()
        {
            GetVrsReticle().Dismiss();
        }

        public void ShowReticle()
        {
            GetVrsReticle().Show();
        }

        private void InitVrsReticleScript()
        {
            if (mVrsReticle == null)
            {
                GameObject vrsReticleObject = GameObject.Find("VrsReticle");
                if (vrsReticleObject != null)
                {
                    mVrsReticle = vrsReticleObject.GetComponent<VrsReticle>();
                    if (mVrsReticle == null)
                    {
                        Debug.LogError("Not Find VrsReticle.cs From GameObject VrsReticle !!!");
                    }
                }
                else
                {
                    Debug.LogError("Not Find VrsReticle GameObject !!!");
                }
            }
        }

        
        
        
        public void ShowHeadControl()
        {
            InitVrsReticleScript();
            if (mVrsReticle != null)
            {
                mVrsReticle.HeadShow();
                Debug.Log("ShowHeadControl");
            }
        }

        
        
        
        public void HideHeadControl()
        {
            InitVrsReticleScript();
            if (mVrsReticle != null)
            {
                mVrsReticle.HeadDismiss();
                Debug.Log("HideHeadControl");
            }
        }

        
        
        
        
        public bool IsAppHandleTriggerEvent
        {
            get { return appHandleTriggerEvent; }
            set { appHandleTriggerEvent = value; }
        }

        [SerializeField] private bool appHandleTriggerEvent = false;


        
        
        
        
        [SerializeField] private bool appHandleXYABEvent = false;
        public bool IsAppHandleXYABEvent
        {
            get { return appHandleXYABEvent; }
            set { appHandleXYABEvent = value; }
        }



        public bool TrackerPosition
        {
            get { return trackerPosition; }
            set { trackerPosition = value; }
        }

        
        
        
        public bool UseThirdPartyPosition
        {
            get { return useThirdPartyPosition; }
            set { useThirdPartyPosition = value; }
        }

        [SerializeField] private bool useThirdPartyPosition = false;

#if UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
        [SerializeField]
        private FrameRate targetFrameRate = FrameRate.FPS_60;
        public FrameRate TargetFrameRate
        {
            get
            {
                return targetFrameRate;
            }
            set
            {
                if (value != targetFrameRate)
                {
                    targetFrameRate = value;
                }
            }
        }

#endif

        
        [SerializeField] public TextureMSAA textureMsaa = TextureMSAA.MSAA_2X;

        public TextureMSAA TextureMSAA
        {
            get { return textureMsaa; }
            set
            {
                if (value != textureMsaa)
                {
                    textureMsaa = value;
                }
            }
        }

        [SerializeField] private bool trackerPosition = true;

        [Serializable]
        public class VrsSettings
        {
            [Tooltip("Change Timewarp Status")] public int timewarpEnabled = -1; 
            [Tooltip("Change Sync Frame Status")] public bool syncFrameEnabled = false;
        }

        [SerializeField] public VrsSettings settings = new VrsViewer.VrsSettings();

        
        [SerializeField] private bool remoteDebug = false;

        public bool RemoteDebug
        {
            get { return remoteDebug; }
            set
            {
                if (value != remoteDebug)
                {
                    remoteDebug = value;
                }
            }
        }

        [SerializeField] private bool remoteController = false;

        public bool RemoteController
        {
            get { return remoteController; }
            set
            {
                if (value != remoteController)
                {
                    remoteController = value;
                }
            }
        }

        
        [SerializeField] private bool showFPS = false;

        public bool ShowFPS
        {
            get { return showFPS; }
            set
            {
                if (value != showFPS)
                {
                    showFPS = value;
                }
            }
        }

        
        [SerializeField] public TextureQuality textureQuality = TextureQuality.Better;

        public TextureQuality TextureQuality
        {
            get { return textureQuality; }
            set
            {
                if (value != textureQuality)
                {
                    textureQuality = value;
                }
            }
        }

        [SerializeField] private bool requestLock = false;

        
        
        
        public bool LockHeadTracker
        {
            get { return requestLock; }
            set
            {
                if (value != requestLock)
                {
                    requestLock = value;
                }
            }
        }

        public bool InitialRecenter { get; set; }

        
        
        
        public void RequestLock()
        {
            if (device != null)
            {
                device.NLockTracker();
            }
        }

        
        
        
        public void RequestUnLock()
        {
            if (device != null)
            {
                device.NUnLockTracker();
            }
        }

        private bool IsNativeGazeShow = false;

        
        
        
        
        public void GazeApi(GazeTag tag)
        {
            GazeApi(tag, "");
        }


        public void TurnOff()
        {
            device.TurnOff();
        }

        public void Reboot()
        {
            device.Reboot();
        }


        
        
        
        
        
        public void GazeApi(GazeTag tag, string param)
        {
            if (device != null)
            {
                bool rslt = device.GazeApi(tag, param);
                if (tag == GazeTag.Show)
                {
                    bool useDFT = USE_DTR && !VrsGlobal.supportDtr;
                    IsNativeGazeShow = useDFT ? true : rslt;
                }
                else if (tag == GazeTag.Hide)
                {
                    IsNativeGazeShow = false;
                }
            }
        }

        
        
        
        
        public void SwitchControllerMode(bool enabled)
        {
            
            if (enabled)
            {
                HeadControl = HeadControl.Controller;
            }
            else
            {
                
                HeadControl = HeadControl.GazeApplication;
            }
        }

        
        
        
        
        
        
        private void SwitchApplicationReticle(bool enabled)
        {
            InitVrsReticleScript();

            bool IsControllerMode = HeadControl == HeadControl.Controller;

            if (enabled)
            {
                if (mVrsReticle != null) mVrsReticle.Show();
                GazeInputModule.gazePointer = mVrsReticle;
            }
            else if (!enabled && (!VrsGlobal.isVR9Platform || IsControllerMode))
            {
                if (mVrsReticle != null)
                {
                    mVrsReticle.Dismiss();
                }

                GazeInputModule.gazePointer = null;
            }

            if (enabled)
            {
                GazeApi(GazeTag.Hide);
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        
        public VrsProfile.ScreenSizes ScreenSize
        {
            get { return screenSize; }
            set
            {
                if (value != screenSize)
                {
                    screenSize = value;
                    if (device != null)
                    {
                        device.UpdateScreenData();
                    }
                }
            }
        }

        [SerializeField] private VrsProfile.ScreenSizes screenSize = VrsProfile.ScreenSizes.Nexus5;

        
        public VrsProfile.ViewerTypes ViewerType
        {
            get { return viewerType; }
            set
            {
                if (value != viewerType)
                {
                    viewerType = value;
                    if (device != null)
                    {
                        device.UpdateScreenData();
                    }
                }
            }
        }

        [SerializeField] private VrsProfile.ViewerTypes viewerType = VrsProfile.ViewerTypes.CardboardMay2015;
#endif

        
        private static BaseARDevice device;

        public RenderTexture GetStereoScreen(int eye)
        {
            
            if (!splitScreenModeEnabled || VrsGlobal.isVR9Platform)
            {
                return null;
            }

            if (eyeStereoScreens[0] == null)
            {
                
                InitEyeStereoScreens();
            }

            if (Application.isEditor || (VrsViewer.USE_DTR && !VrsGlobal.supportDtr))
            {
                
                return eyeStereoScreens[0];
            }

            
            return eyeStereoScreens[eye + _current_texture_index];
        }

        
        public RenderTexture[] eyeStereoScreens = new RenderTexture[_texture_count];

        private void InitEyeStereoScreens()
        {
            InitEyeStereoScreens(-1, -1);
        }

        
        private void InitEyeStereoScreens(int width, int height)
        {
            RealeaseEyeStereoScreens();

#if UNITY_ANDROID || UNITY_EDITOR || UNITY_STANDALONE_WIN
            bool useDFT = VrsViewer.USE_DTR && !VrsGlobal.supportDtr;
            if (!USE_DTR || useDFT || IsWinPlatform)
            {
                
                RenderTexture rendetTexture = device.CreateStereoScreen(width, height);
                if (!rendetTexture.IsCreated())
                {
                    rendetTexture.Create();
                }

                int tid = (int) rendetTexture.GetNativeTexturePtr();
                for (int i = 0; i < _texture_count; i++)
                {
                    eyeStereoScreens[i] = rendetTexture;
                    _texture_ids[i] = tid;
                }
            }
            else
            {
                for (int i = 0; i < _texture_count; i++)
                {
                    eyeStereoScreens[i] = device.CreateStereoScreen(width, height);
                    eyeStereoScreens[i].Create();
                    _texture_ids[i] = (int) eyeStereoScreens[i].GetNativeTexturePtr();
                }
            }
#endif
        }

        
        private void RealeaseEyeStereoScreens()
        {
            for (int i = 0; i < _texture_count; i++)
            {
                if (eyeStereoScreens[i] != null)
                {
                    eyeStereoScreens[i].Release();
                    eyeStereoScreens[i] = null;
                    _texture_ids[i] = 0;
                }
            }

            Resources.UnloadUnusedAssets();
            Debug.Log("RealeaseEyeStereoScreens");
        }

        
        public VrsProfile Profile
        {
            get { return device.Profile; }
        }

        
        public enum Eye
        {
            Left,

            
            Right,

            
            Center 
        }

        
        
        
        public enum Distortion
        {
            Distorted,

            
            Undistorted 
        }

        
        public Pose3D HeadPose
        {
            get { return device.GetHeadPose(); }
        }

        
        
        
        
        public Matrix4x4 Projection(Eye eye, Distortion distortion = Distortion.Distorted)
        {
            return device.GetProjection(eye, distortion);
        }

        
        
        
        
        public Rect Viewport(Eye eye, Distortion distortion = Distortion.Distorted)
        {
            return device.GetViewport(eye, distortion);
        }

        private void InitDevice()
        {
            if (device != null)
            {
                device.Destroy();
            }

            
            device = BaseARDevice.GetDevice();
            device.Init();

            device.SetSplitScreenModeEnabled(splitScreenModeEnabled);
            
            device.UpdateScreenData();

            GazeApi(GazeTag.Show);
            GazeApi(GazeTag.Set_Size, ((int) GazeSize.Original).ToString());
        }

        
        public bool IsWinPlatform { get; set; }

        ViarusInput vrsInput;

        
        
        
        
        void Awake()
        {
            Debug.Log("VrsViewer Awake");
            SettingsAssetConfig asset;
#if UNITY_EDITOR
            asset = VrsSDKApi.Instance.GetSettingsAssetConfig();
#else
            asset = Resources.Load<SettingsAssetConfig>("Config/SettingsAssetConfig");
#endif
            sixDofMode = asset.mSixDofMode;
            sleepTimeoutMode = asset.mSleepTimeoutMode;
            headControlEnabled = asset.mHeadControl;
            textureQuality = asset.mTextureQuality;
            textureMsaa = asset.mTextureMSAA;
            InitialRecenter = true;
            vrsInput = new ViarusInput();
            IsWinPlatform = false;
            Debug.Log("SettingsAssetConfig:" + asset.mSixDofMode + "--" + asset.mSleepTimeoutMode + "--" +
                      asset.mHeadControl + "--" + asset.mTextureQuality + "--" + asset.mTextureMSAA);
#if UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
            IsWinPlatform = true;
#endif
            if (instance == null)
            {
                instance = this;

                Loom.Initialize();

                if (Application.isMobilePlatform)
                {
                    QualitySettings.antiAliasing = 0;
                    Application.runInBackground = false;
                    Input.gyro.enabled = false;
                    Debug.Log("SleepTimeout:" + SleepMode.ToString());
                    if (SleepMode == SleepTimeoutMode.NEVER_SLEEP)
                    {
                        
                        Screen.sleepTimeout = SleepTimeout.NeverSleep;
                    }
                    else
                    {
                        Screen.sleepTimeout = SleepTimeout.SystemSetting;
                    }
                }
            }

            if (instance != this)
            {
                Debug.LogError("There must be only one VrsViewer object in a scene.");
                DestroyImmediate(this);
                return;
            }

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;

            InitDevice();
            if (!IsWinPlatform && !VrsGlobal.supportDtr && !VrsGlobal.isVR9Platform)
            {
                
                
                AddPrePostRenderStages();
            }

            Debug.Log("Is Windows Platform : " + IsWinPlatform + ", ScreenInfo : " + Screen.width + "*" +
                      Screen.height + ", AntiAliasing : " + QualitySettings.antiAliasing);
#if UNITY_ANDROID
            
            int targetFrameRate = Application.platform == RuntimePlatform.Android
                ? ((int) VrsGlobal.refreshRate > 0 ? (int) VrsGlobal.refreshRate : 90)
                : 60;
            if (VrsGlobal.isVR9Platform)
            {
                
                targetFrameRate = 60;
                textureMsaa = TextureMSAA.NONE;
            }

            Application.targetFrameRate = targetFrameRate;
#endif
            if (Application.platform != RuntimePlatform.Android)
            {
                VrsSDKApi.Instance.IsIVrsMode = true;
            }

            
            if (!VrsGlobal.supportDtr || VrsGlobal.isVR9Platform)
            {
                QualitySettings.vSyncCount = 1;
                if (VrsGlobal.offaxisDistortionEnabled)
                {
                    Application.targetFrameRate = Application.platform == RuntimePlatform.Android
                        ? (int) VrsGlobal.refreshRate
                        : -1;
                    Debug.Log("offaxisDistortionEnabled : Setting frame rate to " + Application.targetFrameRate);
                }
            }
            else
            {
                
                QualitySettings.vSyncCount = 0;
            }
#if UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
            if (IsWinPlatform)
            {
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = (int)targetFrameRate;
                
                Application.runInBackground = true;
                QualitySettings.maxQueuedFrames = -1;
                QualitySettings.antiAliasing = Mathf.Max(QualitySettings.antiAliasing, (int)TextureMSAA);
            }
#endif

#if UNITY_ANDROID && UNITY_EDITOR
            
            
            
            
            
            
            
            
            

#if UNITY_EDITOR
            string defineSymbols =
                UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Android);
            if (defineSymbols == null || !defineSymbols.Contains("VIARUS_"))
            {
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(UnityEditor.BuildTargetGroup.Android, "VIARUS_VR");
            }
#endif
#endif
            device.AndroidLog("Welcome to use Unity VRS SDK , current SDK VERSION is " + VRS_SDK_VERSION + ", j " +
                              VrsGlobal.jarVersion
                              + ", s " + VrsGlobal.soVersion + ", u " + Application.unityVersion + ", fps " +
                              Application.targetFrameRate + ", vsync "
                              + QualitySettings.vSyncCount + ", hmd " + HmdType + ", antiAliasing : " +
                              QualitySettings.antiAliasing
                              + ", SixDofMode : " + sixDofMode.ToString());

            AddStereoControllerToCameras();
        }

        public delegate void ViarusConfigCallback(VrsInstantNativeApi.Viarus_Config cfg);

        ViarusConfigCallback _nvrConfigCallback;

        void Start()
        {
            
            

            if (IsWinPlatform)
            {
                
                _nvrConfigCallback += OnViarusConfigCallback;
                VrsInstantNativeApi.SetViarusConfigCallback(_nvrConfigCallback);


#if VIARUS_DEBUG
                NvrInstantNativeApi.Inited = false;
                Debug.Log("NvrInstantNativeApi.Init.Not Called.");
#else
                int _textureWidth = 1920, _textureHeight = 1080;
                VrsInstantNativeApi.NvrInitParams param;
                param.renderWidth = _textureWidth;
                param.renderHeight = _textureHeight;
                param.bitRate = 30;
                VrsInstantNativeApi.Inited = VrsInstantNativeApi.Init(param);
                Debug.Log("VrsInstantNativeApi.Init.Called.");

                VrsInstantNativeApi.GetVersionInfo(ref VrsInstantNativeApi.nativeApiVersion,
                    ref VrsInstantNativeApi.driverVersion);
                Debug.Log("VrsInstantNativeApi.Version.Api." + VrsInstantNativeApi.nativeApiVersion + ",Driver." +
                          VrsInstantNativeApi.driverVersion);

                if (VrsInstantNativeApi.nativeApiVersion >= 2000)
                {
                    VrsInstantNativeApi.GetTextureResolution(ref _textureWidth, ref _textureHeight);
                    if (VrsInstantNativeApi.driverVersion >= 2002)
                    {
                        UInt32 rateData = VrsInstantNativeApi.GetRefreshRate();
                        Debug.Log("-------------rateData--------" + rateData);
                        if (rateData >= 60)
                        {
                            Application.targetFrameRate = (int) rateData;
                        }
                    }
                }

                if (eyeStereoScreens[0] == null && !VrsGlobal.isVR9Platform)
                {
                    
                    InitEyeStereoScreens(_textureWidth, _textureHeight);
                }

                if (eyeStereoScreens[0] != null)
                {
                    
                    VrsInstantNativeApi.SetFrameTexture(eyeStereoScreens[0].GetNativeTexturePtr());
                    Debug.Log("VrsInstantNativeApi.SetFrameTexture." + eyeStereoScreens[0].GetNativeTexturePtr());
                }
#endif

                Debug.Log("VrsInstantNativeApi.Init. Size " + param.renderWidth + "*" + param.renderHeight + ", Bit " +
                          param.bitRate + ", Inited " + VrsInstantNativeApi.Inited);
            }
            else
            {
                if (eyeStereoScreens[0] == null && !VrsGlobal.isVR9Platform)
                {
                    
                    InitEyeStereoScreens();
                    device.SetTextureSizeNative(eyeStereoScreens[0].width, eyeStereoScreens[1].height);
                }
            }

            if (ShowFPS)
            {
                Transform[] father;
                father = GetComponentsInChildren<Transform>(true);
                GameObject FPS = null;
                foreach (Transform child in father)
                {
                    if (child.gameObject.name == "FPScounter")
                    {
                        FPS = child.gameObject;
                        break;
                    }
                }

                if (FPS != null)
                {
                    FPS.SetActive(ShowFPS);
                }
                else
                {
                    GameObject fpsGo = Instantiate(Resources.Load("Prefabs/FPScounter")) as GameObject;
#if UNITY_ANDROID && !UNITY_EDITOR
                    fpsGo.GetComponent<FPScounter>().enabled = false;
                    fpsGo.AddComponent<FpsStatistics>();
#else
                    fpsGo.GetComponent<FPScounter>().enabled = true;
#endif
                }
            }

            UpdateHeadControl();

            VrsSDKApi.Instance.SixDofControllerPrimaryDeviceType = device.GetSixDofControllerPrimaryDeviceType();
        }

        public void UpdateHeadControl()
        {
            
            
            switch (HeadControl)
            {
                case HeadControl.GazeApplication:
                    SwitchApplicationReticle(true);
                    GetVrsReticle().HeadDismiss();
                    break;
                case HeadControl.GazeSystem:
                    SwitchApplicationReticle(false);
                    GetVrsReticle().HeadDismiss();
                    GazeApi(GazeTag.Show);
                    break;
                case HeadControl.Hover:
                    GetVrsReticle().HeadShow();
                    SwitchApplicationReticle(true);
                    break;
                case HeadControl.Controller:
                    SwitchApplicationReticle(false);
                    GetVrsReticle().HeadDismiss();
                    GazeApi(GazeTag.Hide);
                    break;
            }
        }

        private VrsHead head;

        
        
        
        
        public VrsHead GetHead()
        {
            if (head == null && Controller != null)
            {
                head = Controller.Head;
            }

            if (head == null)
            {
                head = FindObjectOfType<VrsHead>();
            }

            return head;
        }

        
        
        
        
        public void Update3rdPartyPosition(Vector3 position)
        {
            VrsHead mHead = GetHead();
            if (mHead != null)
            {
                mHead.Update3rdPartyPosition(position);
            }
        }

        private void OnViarusConfigCallback(VrsInstantNativeApi.Viarus_Config cfg)
        {
            Loom.QueueOnMainThread((param) =>
            {
                device.Profile.viewer.lenses.separation = cfg.ipd;

                
                device.ComputeEyesForWin(VrsViewer.Eye.Left, cfg.near, 1000, cfg.eyeFrustumParams[0],
                    cfg.eyeFrustumParams[3], cfg.eyeFrustumParams[1], cfg.eyeFrustumParams[2]);
                
                device.ComputeEyesForWin(VrsViewer.Eye.Right, cfg.near, 1000, cfg.eyeFrustumParams[4],
                    cfg.eyeFrustumParams[7], cfg.eyeFrustumParams[5], cfg.eyeFrustumParams[6]);

                
                VrsControllerHelper.InitController((int) cfg.controllerType);
                device.profileChanged = true;
                Debug.Log("OnViarusConfigCallback Config : Ipd " + cfg.ipd + ", Near " + cfg.near +
                          ", FrustumLeft(LRBT) " + cfg.eyeFrustumParams[0] + ", " + cfg.eyeFrustumParams[1] + ","
                          + cfg.eyeFrustumParams[2] + ", " + cfg.eyeFrustumParams[3] + ", ControllerType " +
                          cfg.controllerType);

                TrackerPosition = true;
                {
                    VrsHead head = GetHead();
                    if (head != null)
                    {
                        head.SetTrackPosition(TrackerPosition);
                    }
                }

                device.profileChanged = true;
            }, cfg);
        }

        private int baseTryTimes = 1;

        private void Update()
        {

          UpdateHeadControl();
          
            UpdateState();
            if (!VrsGlobal.isVR9Platform)
            {
                UpdateEyeTexture();
            }


            if (GazeInputModule.gazePointer != null)
            {
                GazeInputModule.gazePointer.UpdateStatus();
            }

            if (vrsInput != null)
            {
                vrsInput.Process();
            }

            if (OnUpdateActionHandler!=null)
            {
                OnUpdateActionHandler();
            }
        }

        public BaseARDevice GetDevice()
        {
            return device;
        }

        
        
        
        
        public void AndroidLog(string msg)
        {
            if (device != null)
            {
                device.AndroidLog(msg);
            }
            else
            {
                Debug.Log(msg);
            }
        }

        public void UpdateHeadPose()
        {
            if (device != null && VrsSDKApi.Instance.IsIVrsMode)
                device.UpdateState();
        }

        public void UpdateEyeTexture()
        {
            
            if (USE_DTR && VrsGlobal.supportDtr)
            {
                
                SwapBuffers();

                VrsEye[] eyes = VrsViewer.Instance.eyes;
                for (int i = 0; i < 2; i++)
                {
                    VrsEye eye = eyes[i];
                    if (eye != null)
                    {
                        eye.UpdateTargetTexture();
                    }
                }
            }
        }

        void AddPrePostRenderStages()
        {
            var preRender = FindObjectOfType<VrsPreRender>();
            if (preRender == null)
            {
                var go = new GameObject("PreRender", typeof(VrsPreRender));
                go.SendMessage("Reset");
                go.transform.parent = transform;
                Debug.Log("Add VrsPreRender");
            }

            var postRender = FindObjectOfType<VrsPostRender>();
            if (postRender == null)
            {
                var go = new GameObject("PostRender", typeof(VrsPostRender));
                go.SendMessage("Reset");
                go.transform.parent = transform;
                Debug.Log("Add VrsPostRender");
            }
        }

        
        
        public bool Triggered { get; set; }

        public bool ProfileChanged { get; private set; }

        
        private int updatedToFrame = 0;

        
        
        
        
        
        
        public void UpdateState()
        {
            if (updatedToFrame != Time.frameCount)
            {
                updatedToFrame = Time.frameCount;
                DispatchEvents();
                if (NeedUpdateNearFar && device != null && device.viarusVRServiceId != 0)
                {
                    float far = GetCameraFar();
                    float mNear = 0.0305f;
                    if (VrsGlobal.fovNear > -1)
                    {
                        mNear = VrsGlobal.fovNear;
                    }

                    device.SetCameraNearFar(mNear, far);
                    Instance.NeedUpdateNearFar = false;

                    for (int i = 0; i < 2; i++)
                    {
                        VrsEye eye = eyes[i];
                        if (eye != null)
                        {
                            if (eye.cam.farClipPlane < VrsGlobal.fovFar)
                            {
                                eye.cam.farClipPlane = VrsGlobal.fovFar;
                            }
                        }
                    }
                }
            }
        }

        int[] lastKeyAction;

        private void DispatchEvents()
        {
            
            if (device == null) return;
            ProfileChanged = device.profileChanged;
            if (device.profileChanged)
            {
                if (VrsOverrideSettings.OnProfileChangedEvent != null) VrsOverrideSettings.OnProfileChangedEvent();
                device.profileChanged = false;
            }

            
            

            bool IsHasController = false;
            if (Application.platform == RuntimePlatform.Android)
            {
                
                IsHasController = (VrsPlayerCtrl.Instance.IsQuatConn() || ControllerAndroid.IsNoloConn());
            }
            else if (IsWinPlatform)
            {
                IsHasController = VrsControllerHelper.Is3DofControllerConnected &&
                                  VrsPlayerCtrl.Instance.GamepadEnabled;
            }

            if (IsHasController)
            {
                int[] KeyAction = null;
                if (InteractionManager.IsControllerConnected())
                {
                    KeyAction = InteractionManager.GetKeyAction();
                }
                else
                {
                    KeyAction = ViarusTaskApi.GetKeyAction();
                }

                if (lastKeyAction == null)
                {
                    lastKeyAction = KeyAction;
                }

                for (int i = 0; i < CKeyEvent.KeyCodeIds.Length; i++)
                {
                    int keyCode = CKeyEvent.KeyCodeIds[i];

                    if (KeyAction[keyCode] == 0 && lastKeyAction[keyCode] == 1)
                    {
                        
                        
                    }

                    if (KeyAction[keyCode] == 1 && lastKeyAction[keyCode] == 0)
                    {
                        
                        
                    }
                }

                lastKeyAction = KeyAction;
            }

            
            
            float leftKeyHor = Input.GetAxis("5th axis");
            float leftKeyVer = Input.GetAxis("6th axis");

            if (Application.platform == RuntimePlatform.Android && Event.current != null &&
                (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow)
                                                   || Input.GetKeyUp(KeyCode.DownArrow) ||
                                                   Input.GetKeyUp(KeyCode.UpArrow)
                                                   || Input.GetKeyUp(KeyCode.Escape)))
            {
                Debug.Log("KeyUp===>" + Event.current.keyCode.ToString());
            }
        }
        

        
        
        
        public void Recenter()
        {
            device.Recenter();
            if (GetHead() != null)
            {
                GetHead().ResetInitEulerYAngle();
            }
        }

        
        
        public static void AddStereoControllerToCameras()
        {
            for (int i = 0; i < Camera.allCameras.Length; i++)
            {
                Camera camera = Camera.allCameras[i];
                Debug.Log("Check Camera : " + camera.name);
                if (
                    (camera.tag == "MainCamera")
                    && camera.targetTexture == null &&
                    camera.GetComponent<VrsStereoController>() == null &&
                    camera.GetComponent<VrsEye>() == null &&
                    camera.GetComponent<VrsPreRender>() == null &&
                    camera.GetComponent<VrsPostRender>() == null)
                {
                    camera.gameObject.AddComponent<VrsStereoController>();
                }
            }
        }

        void OnEnable()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            
            if (device == null)
            {
                InitDevice();
            }
#endif
            device.OnPause(false);
            if (!VrsGlobal.isVR9Platform)
            {
                StartCoroutine("EndOfFrame");
            }
        }

        void OnDisable()
        {
            device.OnPause(true);
            Debug.Log("VrsViewer->OnDisable");
            StopCoroutine("EndOfFrame");
        }

        private Coroutine onResume = null;

        void OnPause()
        {
            onResume = null;
            device.OnApplicationPause(true);
        }

        IEnumerator OnResume()
        {
            yield return new WaitForSeconds(1.0f);
            
            if (!VrsGlobal.isVR9Platform && VrsGlobal.supportDtr)
            {
                InitVrsReticleScript();
                UpdateHeadControl();
            }

            device.OnApplicationPause(false);
        }

        public void SetPause(bool pause)
        {
            if (pause)
            {
                OnPause();
            }
            else if (onResume == null)
            {
                onResume = StartCoroutine(OnResume());
            }
        }

        void OnApplicationPause(bool pause)
        {
            Debug.Log("VrsViewer->OnApplicationPause," + pause + ", hasEnterXRMode=" + VrsSDKApi.Instance.IsIVrsMode);
            SetPause(pause);
        }

        void OnApplicationFocus(bool focus)
        {
            Debug.Log("VrsViewer->OnApplicationFocus," + focus);
            device.OnFocus(focus);
        }

        void OnApplicationQuit()
        {
            if (GetViarusService() != null && GetViarusService().IsMarkerRecognizeRunning)
            {
                GetViarusService().StopMarkerRecognize();
            }

            StopAllCoroutines();
            device.OnApplicationQuit();

            if (VrsOverrideSettings.OnApplicationQuitEvent != null)
            {
                VrsOverrideSettings.OnApplicationQuitEvent();
            }

            Debug.Log("VrsViewer->OnApplicationQuit");

#if UNITY_ANDROID && !UNITY_EDITOR
			if(IsAndroidKillProcess) 
            {
                 VrsSDKApi.Instance.Destroy();
                 Debug.Log("VrsViewer->OnApplicationQuit.KillProcess");
                 System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
#endif
        }

        
        
        
        public void AppQuit()
        {
            device.AppQuit();
        }

        void OnDestroy()
        {
            if (IsWinPlatform)
            {
                if (VrsInstantNativeApi.Inited)
                {
                    _nvrConfigCallback -= OnViarusConfigCallback;
                    VrsInstantNativeApi.Inited = false;
                    VrsInstantNativeApi.Cleanup();
                    VrsControllerHelper.Reset();
                }
            }

            this.SplitScreenModeEnabled = false;
            InteractionManager.Reset();
            if (device != null)
            {
                device.Destroy();
            }

            if (instance == this)
            {
                instance = null;
            }

            Debug.Log("VrsViewer->OnDestroy");
        }

        
        public void ResetHeadTrackerFromAndroid()
        {
            if (instance != null && device != null)
            {
                Recenter();
            }
        }

        void OnVolumnUp()
        {
            if (vrsInput != null)
            {
                vrsInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_VOLUME_DOWN, CKeyEvent.ACTION_DOWN);
            }
        }

        void OnVolumnDown()
        {
            if (vrsInput != null)
            {
                vrsInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_VOLUME_UP, CKeyEvent.ACTION_DOWN);
            }
        }

        void OnKeyDown(string keyCode)
        {
            Debug.Log("OnKeyDown=" + keyCode);
            if (keyCode == VrsGlobal.KeyEvent_KEYCODE_ALT_LEFT)
            {
                if (vrsInput != null)
                {
                    vrsInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_NF_1, CKeyEvent.ACTION_DOWN);
                }
            }
            else if (keyCode == VrsGlobal.KeyEvent_KEYCODE_MEDIA_RECORD)
            {
                if (vrsInput != null)
                {
                    vrsInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_NF_2, CKeyEvent.ACTION_DOWN);
                }
            }
        }

        void OnKeyUp(string keyCode)
        {
            Debug.Log("OnKeyUp=" + keyCode);
            if (keyCode == VrsGlobal.KeyEvent_KEYCODE_ALT_LEFT)
            {
                if (vrsInput != null)
                {
                    vrsInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_NF_1, CKeyEvent.ACTION_UP);
                }
            }
            else if (keyCode == VrsGlobal.KeyEvent_KEYCODE_MEDIA_RECORD)
            {
                if (vrsInput != null)
                {
                    vrsInput.OnChangeKeyEvent(CKeyEvent.KEYCODE_NF_2, CKeyEvent.ACTION_UP);
                }
            }
        }

        void OnActivityPause()
        {
            Debug.Log("OnActivityPause");
        }

        void OnActivityResume()
        {
            Debug.Log("OnActivityResume");
        }

        
        
        
        
        public void SetSystemSplitMode(int flag)
        {
            device.NSetSystemSplitMode(flag);
        }

        private int[] _texture_ids = new int[_texture_count];
        private int _current_texture_index, _next_texture_index;

        public bool SwapBuffers()
        {
            bool ret = true;
            for (int i = 0; i < _texture_count; i++)
            {
                if (!eyeStereoScreens[i].IsCreated())
                {
                    eyeStereoScreens[i].Create();
                    _texture_ids[i] = (int) eyeStereoScreens[i].GetNativeTexturePtr();
                    ret = false;
                }
            }

            _current_texture_index = _next_texture_index;
            _next_texture_index = (_next_texture_index + 2) % _texture_count;
            return ret;
        }

        public int GetEyeTextureId(int eye)
        {
            return _texture_ids[_current_texture_index + (int) eye];
        }

        public int GetTimeWarpViewNum()
        {
            return device.GetTimewarpViewNumber();
        }

        public List<GameObject> GetAllObjectsInScene()
        {
            GameObject[] pAllObjects = (GameObject[]) Resources.FindObjectsOfTypeAll(typeof(GameObject));
            List<GameObject> pReturn = new List<GameObject>();
            foreach (GameObject pObject in pAllObjects)
            {
                if (pObject == null || !pObject.activeInHierarchy || pObject.hideFlags == HideFlags.NotEditable ||
                    pObject.hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }

                pReturn.Add(pObject);
            }

            return pReturn;
        }

        public Texture2D createTexture2D(RenderTexture renderTexture)
        {
            int width = renderTexture.width;
            int height = renderTexture.height;
            Texture2D texture2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture2D.Apply();
            return texture2D;
        }

        private int frameCount = 0;

        private void EndOfFrameCore()
        {
            if (USE_DTR && (!VrsSDKApi.Instance.IsIVrsMode && frameCount < 3))
            {
                frameCount++;
                Debug.Log("EndOfFrame->hasEnterRMode " + "" + VrsSDKApi.Instance.IsIVrsMode + " or frameCount " +
                          frameCount);
                
                GL.Clear(false, true, Color.black);
            }
            else
            {
                frameCount++;
                if (USE_DTR && VrsGlobal.supportDtr)
                {
                    if (settings.timewarpEnabled >= 0 && frameCount > 0 && frameCount < 10)
                    {
                        device.SetTimeWarpEnable(false);
                    }

                    if (VrsGlobal.DEBUG_LOG_ENABLED) Debug.Log("EndOfFrame.TimeWarp[" + frameCount + "]");
                    
                }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
                
                if (VrsInstantNativeApi.Inited)
                {
                    GL.IssuePluginEvent(VrsInstantNativeApi.GetRenderEventFunc(),
                        (int) VrsInstantNativeApi.RenderEvent.SubmitFrame);
                }
#endif
            }

            bool IsHeadPoseUpdated = device.IsHeadPoseUpdated();
            if (USE_DTR && VrsGlobal.supportDtr && IsHeadPoseUpdated)
                VrsPluginEvent.IssueWithData(ViarusRenderEventType.TimeWarp, VrsViewer.Instance.GetTimeWarpViewNum());
        }

        IEnumerator EndOfFrame()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                EndOfFrameCore();
            }
        }

        public int GetFrameId()
        {
            return frameCount;
        }

        private float mFar = -1;
        private bool needUpdateNearFar = false;

        
        
        
        
        public void UpateCameraFar(float far)
        {
            mFar = far;
            needUpdateNearFar = true;
            VrsGlobal.fovFar = far;
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                
                Camera.main.farClipPlane = far;
            }
        }

        public float GetCameraFar()
        {
            return mFar;
        }

        public bool NeedUpdateNearFar
        {
            get { return needUpdateNearFar; }
            set
            {
                if (value != needUpdateNearFar)
                {
                    needUpdateNearFar = value;
                }
            }
        }


        private float oldFov = -1;

        private Matrix4x4[] eyeOriginalProjection = null;

        
        
        
        
        public void UpdateEyeCameraProjection(Eye eye)
        {
            if (oldFov != -1 && eye == Eye.Right)
            {
                UpdateCameraFov(oldFov);
            }

            if (!Application.isEditor && device != null && eye == Eye.Right)
            {
                if (mFar > 0)
                {
                    float mNear = 0.0305f;
                    if (VrsGlobal.fovNear > -1)
                    {
                        mNear = VrsGlobal.fovNear;
                    }
                    

                    
                    float fovLeft = mNear * Mathf.Tan(-Profile.viewer.maxFOV.outer * Mathf.Deg2Rad);
                    float fovTop = mNear * Mathf.Tan(Profile.viewer.maxFOV.upper * Mathf.Deg2Rad);
                    float fovRight = mNear * Mathf.Tan(Profile.viewer.maxFOV.inner * Mathf.Deg2Rad);
                    float fovBottom = mNear * Mathf.Tan(-Profile.viewer.maxFOV.lower * Mathf.Deg2Rad);

                    

                    Matrix4x4 eyeProjection =
                        BaseARDevice.MakeProjection(fovLeft, fovTop, fovRight, fovBottom, mNear, mFar);
                    for (int i = 0; i < 2; i++)
                    {
                        VrsEye mEye = eyes[i];
                        if (mEye != null)
                        {
                            mEye.cam.projectionMatrix = eyeProjection;
                        }
                    }
                }
            }
        }

        
        
        
        public void ResetCameraFov()
        {
            for (int i = 0; i < 2; i++)
            {
                if (eyeOriginalProjection == null || eyeOriginalProjection[i] == null) return;
                VrsEye eye = eyes[i];
                if (eye != null)
                {
                    eye.cam.projectionMatrix = eyeOriginalProjection[i];
                }
            }

            oldFov = -1;
        }

        
        
        
        
        
        public void UpdateCameraFov(float fov)
        {
            if (fov > 90) fov = 90;
            if (fov < 5) fov = 5;
            
            if (eyeOriginalProjection == null && eyes[0] != null && eyes[1] != null)
            {
                eyeOriginalProjection = new Matrix4x4[2];
                eyeOriginalProjection[0] = eyes[0].cam.projectionMatrix;
                eyeOriginalProjection[1] = eyes[1].cam.projectionMatrix;
            }

            oldFov = fov;
            float near = VrsGlobal.fovNear > 0 ? VrsGlobal.fovNear : 0.0305f;
            float far = VrsGlobal.fovFar > 0 ? VrsGlobal.fovFar : 2000;
            far = far > 100 ? far : 2000;
            float fovLeft = near * Mathf.Tan(-fov * Mathf.Deg2Rad);
            float fovTop = near * Mathf.Tan(fov * Mathf.Deg2Rad);
            float fovRight = near * Mathf.Tan(fov * Mathf.Deg2Rad);
            float fovBottom = near * Mathf.Tan(-fov * Mathf.Deg2Rad);
            Matrix4x4 eyeProjection = BaseARDevice.MakeProjection(fovLeft, fovTop, fovRight, fovBottom, near, far);
            if (device != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    VrsEye eye = eyes[i];
                    if (eye != null)
                    {
                        eye.cam.projectionMatrix = eyeProjection;
                    }
                }
            }
        }


        private float displacementCoefficient = 1.0f;

        public float DisplacementCoefficient
        {
            get { return displacementCoefficient; }
            set { displacementCoefficient = value; }
        }


        
        
        
        
        
        public void SetHorizontalAngleRange(float minRange, float maxRange)
        {
            if (headEulerAnglesRange == null)
            {
                headEulerAnglesRange = new float[] {0, 360, 0, 360};
            }

            headEulerAnglesRange[0] = minRange + 360;
            headEulerAnglesRange[1] = maxRange;
        }

        
        
        
        
        
        public void SetVerticalAngleRange(float minRange, float maxRange)
        {
            if (headEulerAnglesRange == null)
            {
                headEulerAnglesRange = new float[] {0, 360, 0, 360};
            }

            headEulerAnglesRange[2] = minRange + 360;
            headEulerAnglesRange[3] = maxRange;
        }

        
        
        
        public void RemoveAngleLimit()
        {
            headEulerAnglesRange = null;
        }

        public float[] GetHeadEulerAnglesRange()
        {
            return headEulerAnglesRange;
        }

        
        
        
        
        
        
        
        public void OpenVideoPlayer(string path, int type2D3D, int mode, int decode)
        {
            device.ShowVideoPlayer(path, type2D3D, mode, decode);
        }

        
        
        
        
        public string GetStoragePath()
        {
            return device.GetStoragePath();
        }

        
        
        
        
        public void SetIsKeepScreenOn(bool keep)
        {
            device.SetIsKeepScreenOn(keep);
        }

        private float defaultIpd = -1;
        private float userIpd = -1;

        
        
        
        
        public void SetIpd(float ipd)
        {
            if (defaultIpd < 0)
            {
                defaultIpd = GetIpd();
            }

            Debug.Log(" Ipd : D." + defaultIpd + "/N." + ipd);
            VrsGlobal.dftProfileParams[0] = ipd; 
            userIpd = ipd;
            device.SetIpd(ipd);
            device.UpdateScreenData();
        }

        
        
        
        public void ResetIpd()
        {
            if (defaultIpd < 0) return;
            SetIpd(defaultIpd);
        }

        
        
        
        
        public float GetIpd()
        {
            if (userIpd > 0) return userIpd;

            return eyes[0] == null ? 0.060f : 2 * Math.Abs(eyes[0].GetComponent<Camera>().transform.localPosition.x);
        }

        public float GetUseIpd()
        {
            return userIpd;
        }

        public void ShowSystemMenuUI()
        {
            if (device != null)
            {
                
                AndroidJavaClass Player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                
                AndroidJavaObject Activity = Player.GetStatic<AndroidJavaObject>("currentActivity");
                
                AndroidJavaObject PackageNameObj = Activity.Call<AndroidJavaObject>("getPackageName");
                string packageName = PackageNameObj.Call<string>("toString");
                Debug.Log("show_nvr_menu->" + packageName);
                device.SetSystemParameters("show_nvr_menu", packageName);
            }
        }

        public delegate void ServiceReadyUpdatedDelegate(SERVICE_TYPE serviceType, bool isConnectedSucc);

        public delegate void OnSixDofPosition(float x, float y, float z);

        public delegate void OnMarkerLoadStatus(MARKER_LOAD_STATUS status);

        
        
        
        public static ServiceReadyUpdatedDelegate serviceReadyUpdatedDelegate;

        
        
        
        public static OnSixDofPosition onSixDofPosition;

        public static OnMarkerLoadStatus onMarkerLoadStatus;

        
        
        
        
        
        public ViarusService GetViarusService()
        {
            return device.GetViarusService();
        }

        private GameObject eventReceiverVoice;
        private GameObject eventReceiverGesture;

        public void RegisterVoiceListener(GameObject listenerObj)
        {
            eventReceiverVoice = listenerObj;
        }

        public void RegisterGestureListener(GameObject listenerObj)
        {
            eventReceiverGesture = listenerObj;
        }

        void onHandleAndroidMsg(string msgContent)
        {
            
            string[] msgArr = msgContent.Split('_');
            int msgId = int.Parse(msgArr[0]);
            string msgData = msgArr[1];

            
            if (msgId == (int) MSG_ID.MSG_onServiceReady)
            {
                if (serviceReadyUpdatedDelegate != null)
                {
                    SERVICE_TYPE serviceType = (SERVICE_TYPE) int.Parse(msgData);
                    bool connected = int.Parse(msgArr[2]) == 1;
                    serviceReadyUpdatedDelegate(serviceType, connected);
                }

                return;
            }

            if (msgId == (int) MSG_ID.MSG_verifyFailed)
            {
                VrsGlobal.verifyStatus = (VERIFY_STATUS) int.Parse(msgData);
                Debug.Log("verify failed");
            }
            else if (msgId == (int) MSG_ID.MSG_verifySucc)
            {
                VrsGlobal.verifyStatus = VERIFY_STATUS.SUCC;
                Debug.Log("verify succ");
            }
            else if (msgId == (int) MSG_ID.MSG_onKeyStoreException)
            {
                VrsGlobal.verifyStatus = VERIFY_STATUS.HEAD_ERROR;
                Debug.Log("verify keystore exception");
            }
            else if (msgId == (int) MSG_ID.MSG_onHeadPosition && onSixDofPosition != null)
            {
                
                string[] posStr = msgData.Substring(1, msgData.Length - 2).Split(',');
                float x = (float) Math.Round(float.Parse(posStr[0]), 2);
                float y = (float) Math.Round(float.Parse(posStr[1]), 2);
                float z = (float) Math.Round(float.Parse(posStr[2]), 2);
                
                if (onSixDofPosition != null)
                {
                    onSixDofPosition(x, y, z);
                }
            }
            else if ((MSG_ID) msgId == MSG_ID.MSG_onMarkerLoadStatus && onMarkerLoadStatus != null)
            {
                if (onMarkerLoadStatus != null)
                {
                    onMarkerLoadStatus((MARKER_LOAD_STATUS) int.Parse(msgData));
                }
            }

            if ((eventReceiverGesture != null || eventReceiverVoice != null) &&
                VrsGlobal.verifyStatus == VERIFY_STATUS.SUCC)
            {
                object msgObj = null;
                if ((MSG_ID) msgId == MSG_ID.MSG_onGestureEvent)
                {
                    msgObj = (GESTURE_ID) int.Parse(msgData);
                    if (eventReceiverGesture != null)
                        eventReceiverGesture.BroadcastMessage(VrsGlobal.GetMethodNameById((MSG_ID) msgId), msgObj,
                            SendMessageOptions.DontRequireReceiver);
                }
                else if ((MSG_ID) msgId == MSG_ID.MSG_onVoiceVolume ||
                         (MSG_ID) msgId == MSG_ID.MSG_onVoiceFinishError ||
                         (MSG_ID) msgId == MSG_ID.MSG_onVoiceFinishResult)
                {
                    msgObj = msgData;
                    if ((MSG_ID) msgId == MSG_ID.MSG_onVoiceFinishError && msgArr.Length > 1)
                    {
                        
                        msgObj = msgArr[2];
                    }

                    if (eventReceiverVoice != null)
                        eventReceiverVoice.BroadcastMessage(VrsGlobal.GetMethodNameById((MSG_ID) msgId), msgObj,
                            SendMessageOptions.DontRequireReceiver);
                }
            }

            if ((MSG_ID) msgId == MSG_ID.MSG_onServerApiReady)
            {
                Loom.QueueOnMainThread((param) =>
                {
                    bool isReady = int.Parse((string) param) == 1;
                    if (ViarusTaskApi.serverApiReady != null)
                    {
                        ViarusTaskApi.serverApiReady(isReady);
                    }
                }, msgData);
            }
            else if ((MSG_ID) msgId == MSG_ID.MSG_onSysSleepApiReady)
            {
                Loom.QueueOnMainThread((param) =>
                {
                    bool isReady = int.Parse((string) param) == 1;
                    if (ViarusTaskApi.sysSleepApiReady != null)
                    {
                        ViarusTaskApi.sysSleepApiReady(isReady);
                    }
                }, msgData);
            }
            else if ((MSG_ID) msgId == MSG_ID.MSG_onInteractionDeviceConnectEvent)
            {
                InteractionManager.OnDeviceConnectState(msgContent);
            }
            else if ((MSG_ID) msgId == MSG_ID.MSG_onInteractionKeyEvent)
            {
                if (!TouchScreenKeyboard.visible)
                {
                    InteractionManager.OnCKeyEvent(msgContent);
                }
                else
                {
                    Triggered = false;
                }
            }
            else if ((MSG_ID) msgId == MSG_ID.MSG_onInteractionTouchEvent)
            {
                InteractionManager.OnCTouchEvent(msgContent);
            }
        }

        
        
        
        public VOICE_LANGUAGE VoiceLanguage
        {
            get { return VrsGlobal.voiceLanguage; }
            set
            {
                if (value != VrsGlobal.voiceLanguage)
                {
                    VrsGlobal.voiceLanguage = value;
                    ViarusService viarusService = device.GetViarusService();
                    if (viarusService != null)
                    {
                        viarusService.UpdateVoiceLanguage();
                    }
                }
            }
        }

        [SerializeField] public SleepTimeoutMode sleepTimeoutMode = SleepTimeoutMode.NEVER_SLEEP;

        [SerializeField] public ControllerSupportMode controllerSupportMode = ControllerSupportMode.NONE;

        public SleepTimeoutMode SleepMode
        {
            get { return sleepTimeoutMode; }
            set
            {
                if (value != sleepTimeoutMode)
                {
                    sleepTimeoutMode = value;
                }
            }
        }

        [SerializeField] private SixDofMode sixDofMode = SixDofMode.Head_3Dof_Ctrl_6Dof;

        public SixDofMode SixDofMode
        {
            get { return sixDofMode; }
            set
            {
                if (value != sixDofMode)
                {
                    sixDofMode = value;
                }
            }
        }

        
        
        
        
        public Camera GetMainCamera()
        {
            return Controller.cam;
        }

        
        
        
        
        public Camera GetLeftEyeCamera()
        {
            return Controller.Eyes[(int) Eye.Left].cam;
        }

        
        
        
        
        public Camera GetRightEyeCamera()
        {
            return Controller.Eyes[(int) Eye.Right].cam;
        }

        
        
        
        
        public Quaternion GetCameraQuaternion()
        {
            return GetHead().transform.rotation;
        }

        
        
        
        
        public Quaternion GetControllerQuaternion()
        {
            if (IsControllerConnect())
            {
                return VrsPlayerCtrl.Instance.mTransform.localRotation;
            }

            return Quaternion.identity;
        }

        
        
        
        
        public void SetControllerActive(bool isActive)
        {
            VrsPlayerCtrl.Instance.ChangeControllerDisplay(isActive);
        }

        
        
        
        
        public bool IsCameraLocked()
        {
            return !VrsViewer.Instance.GetHead().IsTrackRotation();
        }

        
        
        
        
        public bool IsControllerConnect()
        {
            return InteractionManager.IsControllerConnected();
        }

        
        
        
        
        public void LockCamera(bool isLock)
        {
            VrsHead head = VrsViewer.Instance.GetHead();
            head.SetTrackRotation(!isLock);
        }

        
        
        
        
        public Transform GetRayStartPoint()
        {
            return VrsPlayerCtrl.Instance.GetRayStartPoint();
        }

        
        
        
        
        public Transform GetRayEndPoint()
        {
            return VrsPlayerCtrl.Instance.GetRayEndPoint();
        }

        public Coroutine ViarusStartCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }
        
        public void ViarusStopCoroutine(Coroutine routine)
        {
            StopCoroutine(routine);
        }
    }
}