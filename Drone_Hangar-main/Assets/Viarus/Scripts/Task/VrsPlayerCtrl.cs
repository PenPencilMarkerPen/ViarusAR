using ViarusTask;
using Vrs.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace ViarusAxis
{
    
    
    
    public class VrsPlayerCtrl : MonoBehaviour
    {
        private static VrsPlayerCtrl m_instance = null;

        private bool isCreateControllerHandler = false;

        public bool debugInEditor;

        [SerializeField] private bool gamepadEnabled = true;

        public bool controllerModelDisplay = true;

      
        
        public bool GamepadEnabled
        {
            get { return gamepadEnabled; }
            set { gamepadEnabled = value; }
        }

        public static VrsPlayerCtrl Instance
        {
            get { return m_instance; }
        }

        public Vector3 HeadPosition { get; set; }

        public Transform mTransform;
        Quaternion controllerQuat = new Quaternion(0, 0, 0, 1);
        public Quaternion EditorRemoteQuat = new Quaternion(0, 0, 0, 1);

        public void OnDeviceConnectState(int state, CDevice device)
        {
            Debug.Log("VrsPlayerCtrl.onDeviceConnectState:" + (state == 0 ? " Connect " : " Disconnect "));
            if (state == 0)
            {
                
                VrsViewer.Instance.HideHeadControl();
                VrsViewer.Instance.SwitchControllerMode(true);
            }
        }

        private void Awake()
        {
            m_instance = this;
            mTransform = transform;
        }

        void Start()
        {
            HeadPosition = Vector3.zero;
#if UNITY_ANDROID && !UNITY_EDITOR
           if(!InteractionManager.IsInteractionSDKEnabled())
            {
                ControllerAndroid.onStart();
                ViarusTaskApi.setOnDeviceListener(OnDeviceConnectState);
               
            }
#endif
        }

        public bool IsQuatConn()
        {
            if (debugInEditor) return true;

            if (gamepadEnabled)
            {
                if (VrsControllerHelper.Is3DofControllerConnected)
                {
                    return true;
                }

                if (InteractionManager.IsInteractionSDKEnabled())
                {
                    return InteractionManager.Is3DofControllerConnected();
                }
#if UNITY_ANDROID && !UNITY_EDITOR
                return ControllerAndroid.isQuatConn();
#endif
            }

            return false;
        }

        void Update()
        {
#if UNITY_ANDROID 
            bool isQuatConn = IsQuatConn();
            if (debugInEditor)
            {
                isQuatConn = true;
            }
            
            bool isNeedShowController = isQuatConn ? controllerModelDisplay : false;
            var systemUIControl = VrsViewer.Instance.GetDevice().GetSystemUIControl();
            if (isNeedShowController && systemUIControl == 1)
            {
                isNeedShowController = false;
            }

            if (isQuatConn)
            {
                
                if (InteractionManager.IsControllerConnected())
                {
                    float[] res = InteractionManager.GetControllerPose(InteractionManager.GetHandTypeByHandMode());
                    controllerQuat.x = res[0];
                    controllerQuat.y = res[1];
                    controllerQuat.z = res[2];
                    controllerQuat.w = res[3];
                }
                else
                {
                    float[] res = ControllerAndroid.getQuat(1);
                    controllerQuat.x = res[0];
                    controllerQuat.y = res[1];
                    controllerQuat.z = res[2];
                    controllerQuat.w = res[3];
                }

#if UNITY_EDITOR
                if (VrsViewer.Instance.RemoteDebug && VrsViewer.Instance.RemoteController)
                {
                    controllerQuat = EditorRemoteQuat;
                }
#endif

                
                mTransform.rotation = controllerQuat;

                if (VrsSDKApi.Instance.Is3DofSpriteFirstLoad && !isTipsCreated)
                {
                    CreateTipImgs();
                }
                else if (isTipsCreated)
                {
                    ChangeTipAlpha();
                }
            }

#elif UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
            if (VrsControllerHelper.Is3DofControllerConnected)
            {
                if (!VrsInstantNativeApi.Inited) return;

                _prevStates = _currentStates;
                VrsInstantNativeApi.ViarusDeviceType deviceTypeOf3dof =
                    VrsControllerHelper.HandMode3DOF == VrsControllerHelper.LEFT_HAND_MODE
                        ? VrsInstantNativeApi.ViarusDeviceType.LeftController
                        : VrsInstantNativeApi.ViarusDeviceType.RightController;
                _currentStates = VrsInstantNativeApi.GetControllerStates(deviceTypeOf3dof);


                VrsInstantNativeApi.Viarus_Pose pose =
                    VrsInstantNativeApi.GetPoseByDeviceType(VrsInstantNativeApi.ViarusDeviceType.RightController);
                mTransform.rotation =
                    new Quaternion(pose.rotation.x, pose.rotation.y, pose.rotation.z, pose.rotation.w);

                if (GetButtonDown(VrsTrackedDevice.ButtonID.TouchPad))
                {
                    int[] KeyAction = ViarusTaskApi.GetKeyAction();
                    KeyAction[CKeyEvent.KEYCODE_DPAD_CENTER] = 1;
                }

                if (GetButtonUp(VrsTrackedDevice.ButtonID.TouchPad))
                {
                    int[] KeyAction = ViarusTaskApi.GetKeyAction();
                    KeyAction[CKeyEvent.KEYCODE_DPAD_CENTER] = 0;
                }
            }
#endif
        }

        private VrsInstantNativeApi.Viarus_ControllerStates _prevStates;
        private VrsInstantNativeApi.Viarus_ControllerStates _currentStates;

        bool GetButtonDown(VrsTrackedDevice.ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int) btn)) != 0 && (_prevStates.buttons & (1 << (int) btn)) == 0;
        }

        bool GetButtonUp(VrsTrackedDevice.ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int) btn)) == 0 && (_prevStates.buttons & (1 << (int) btn)) != 0;
        }

        void OnApplicationPause()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ControllerAndroid.onPause();
#endif
        }

        void OnApplicationQuit()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            ControllerAndroid.onStop();
#endif
        }

        
        
        
        
        public void DestroyChild(Transform _trsParent)
        {
            for (int i = 0; i < _trsParent.childCount; i++)
            {
                GameObject go = _trsParent.GetChild(i).gameObject;
                Destroy(go);
            }
        }

        string Controller_Name_DEFAULT = "Handler_01";
        string Controller_Name_XIMMERSE = "Handler_03";
        string Controller_Name_CLEER = "Handler_04";

        string GetControllerName()
        {
            int deviceType = ControllerAndroid.getDeviceType();
            if (deviceType == CDevice.DEVICE_CLEER)
            {
                return Controller_Name_CLEER;
            }
            else if (deviceType == CDevice.DEVICE_XIMMERSE)
            {
                return Controller_Name_XIMMERSE;
            }

            return Controller_Name_DEFAULT;
        }

        bool DebugCtrlModelInEditor = false;
        public bool isNeedCustomModel;
        public string customModelPrefabName = "CustomModel";

         VrsTrackedDevice trackedDevice = null;

        public VrsLaserPointer GetControllerLaser()
        {
            if (trackedDevice == null)
            {
                trackedDevice = GetComponent<VrsTrackedDevice>();
            }

            if (trackedDevice == null)
            {
                return null;
            }

            if (trackedDevice.GetLaserPointer() == null)
            {
                return null;
            }

            return trackedDevice.GetLaserPointer();
        }

        public GameObject GetControllerLaserDot()
        {
            if (trackedDevice == null)
            {
                trackedDevice = GetComponent<VrsTrackedDevice>();
            }

            if (trackedDevice == null)
            {
                return null;
            }

            if (trackedDevice.GetLaserPointer() == null)
            {
                return null;
            }

            return trackedDevice.GetLaserPointer().GetLosDot();
        }

        public void ChangeControllerDisplay(bool show)
        {
            controllerModelDisplay = show;
        }

        private GameObject tipsGo;
        private bool isTipsCreated;
        private string lastModelPath;

        
        
        
        
        public void CreateControllerTips(Transform tipsParentTransform)
        {
            if(tipsGo) Destroy(tipsGo);  
#if UNITY_EDITOR
            tipsGo = Instantiate(Resources.Load("Controller/Objs/26/Canvas"), tipsParentTransform) as GameObject;
            tipsGo.transform.localPosition = Vector3.zero;
#elif UNITY_ANDROID
            Debug.Log("IsSptControllerTipUI："+VrsSDKApi.Instance.IsSptControllerTipUI());
            if (!VrsSDKApi.Instance.IsSptControllerTipUI()) return;
            tipsGo = new GameObject("Canvas");
            tipsGo.transform.SetParent(tipsParentTransform);
            var canvas = tipsGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            tipsGo.AddComponent<CanvasScaler>();
            tipsGo.AddComponent<GraphicRaycaster>();
            tipsGo.transform.localPosition = Vector3.zero;
            tipsGo.transform.localRotation = Quaternion.Euler(75, 0, 0);
            tipsGo.transform.localScale = Vector3.one * 0.001f;
            tipsGo.AddComponent<CanvasGroup>();
            Debug.Log("----------The GameObject of ControllerTips is created----------");
#endif
        }

        
        
        
        public void CreateTipImgs()
        {
            var controllerKeyInfoList = InteractionManager.GetControllerConfig().KeyInfoList;
            for (var i = 0; i < controllerKeyInfoList.Count; i++)
            {
                var tipGo = new GameObject(controllerKeyInfoList[i].desc);
                tipGo.transform.SetParent(tipsGo.transform);
                var img = tipGo.AddComponent<Image>();
                var tipList = controllerKeyInfoList[i].tipList;
                var currentControllerTip = tipList[(int) VrsSDKApi.Instance.GetTipLanguage()];
                var pos = currentControllerTip.position;
                var rotation = currentControllerTip.rotation;
                var scale = currentControllerTip.size;
                img.sprite = VrsSDKApi.Instance.GetSprite(currentControllerTip.picPath);
                img.SetNativeSize();
                img.transform.localPosition = new Vector3(pos[0], pos[1], pos[2]);
                img.transform.localRotation = new Quaternion(rotation[0], rotation[1], rotation[2], 1);
                img.transform.localScale = new Vector2(scale[0], scale[1]);
            }

            isTipsCreated = true;
            Debug.Log("----------The UI of ControllerTips is created----------");
        }

        
        
        
        public void ChangeTipAlpha()
        {
            if (!tipsGo) return;
            var parentRotationX = tipsGo.transform.parent.parent.localRotation.x;
            var tipsAlpha = parentRotationX / 0.5f;
            if (parentRotationX >= 0.0f && parentRotationX <= 0.5f)
            {
                tipsAlpha = Mathf.Clamp(tipsAlpha, 0.0f, 1.0f);
            }
            else
            {
                tipsAlpha = 0.0f;
            }

            tipsGo.GetComponent<CanvasGroup>().alpha = tipsAlpha;
            
        }

        public bool IsControllerExist()
        {
            return isCreateControllerHandler;
        }

        public Transform GetRayStartPoint()
        {
            return transform.GetChild(0);
        }

        public Transform GetRayEndPoint()
        {
            return GetControllerLaserDot().transform;
        }

        public Quaternion GetControllerQuaternion()
        {
            if (isCreateControllerHandler)
            {
                return transform.rotation;
            }

            return Quaternion.identity;
        }
    }
}