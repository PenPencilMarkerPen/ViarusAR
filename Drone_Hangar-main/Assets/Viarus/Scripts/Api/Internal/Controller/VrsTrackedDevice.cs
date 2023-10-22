using ViarusAxis;
using ViarusTask;
using UnityEngine;

namespace Vrs.Internal
{
    public class VrsTrackedDevice : MonoBehaviour
    {
        public enum ButtonID
        {
            Trigger = 0,
            Grip = 1,
            Menu = 21,
            System = -1,
            TouchPad = 20,
            DPadUp = 5,
            DPadDown = 4,
            DPadLeft = 2,
            DPadRight = 3,
            DPadCenter = 6,
            TrackpadTouch = 7,
            InternalTrigger = 8,
        }   

        public VrsInstantNativeApi.ViarusDeviceType deviceType;
        VrsInstantNativeApi.Viarus_ControllerStates _prevStates;
        VrsInstantNativeApi.Viarus_ControllerStates _currentStates;

        VrsLaserPointer laserPointer;
        public bool isGamePad;
        public bool DebugInEditor;

        public VrsLaserPointer GetLaserPointer()
        {
            return laserPointer;
        }

        public void ReloadLaserPointer(VrsLaserPointer laserPointerIn)
        {
            this.laserPointer = laserPointerIn;
            if (laserPointer != null)
            {
                laserPointer.PointerIn += PointerInEventHandler;
                laserPointer.PointerOut += PointerOutEventHandler;
            }
        }

        private void Start()
        {
            isGamePad = gameObject.name.Contains("Gamepad");

            laserPointer = GetComponent<VrsLaserPointer>();
            if (laserPointer != null)
            {
                laserPointer.PointerIn += PointerInEventHandler;
                laserPointer.PointerOut += PointerOutEventHandler;
            }
#if UNITY_ANDROID
            ViarusTaskApi.deviceConnectState += OnDeviceConnectState;
            _currentStates = new VrsInstantNativeApi.Viarus_ControllerStates();
            _prevStates = new VrsInstantNativeApi.Viarus_ControllerStates();
            _currentStates.connectStatus = 0;
            _prevStates.connectStatus = 0;

            _currentStates.buttons = 0;
            _currentStates.touches = 0;
            _prevStates.buttons = 0;
            _prevStates.touches = 0;
#endif
        }

        private void OnDestroy()
        {
            if (laserPointer != null)
            {
                laserPointer.PointerIn -= PointerInEventHandler;
                laserPointer.PointerOut -= PointerOutEventHandler;
            }
#if UNITY_ANDROID
            ViarusTaskApi.deviceConnectState -= OnDeviceConnectState;
#endif
        }

        private int GetNoloType()
        {
            int noloType = (int) CDevice.NOLO_TYPE.NONE;
            if (deviceType == VrsInstantNativeApi.ViarusDeviceType.LeftController)
            {
                noloType = (int) CDevice.NOLO_TYPE.LEFT;
            }
            else if (deviceType == VrsInstantNativeApi.ViarusDeviceType.RightController)
            {
                noloType = (int) CDevice.NOLO_TYPE.RIGHT;
            }
            else if (deviceType == VrsInstantNativeApi.ViarusDeviceType.Hmd)
            {
                noloType = (int) CDevice.NOLO_TYPE.HEAD;
            }

            return noloType;
        }

        public void OnDeviceConnectState(int state, CDevice device)
        {
            if (device.getType() != CDevice.DEVICE_NOLO_SIXDOF) return;
            
            Debug.Log("VrsTrackedDevice.onDeviceConnectState:" + state + "," + device.getType() + "," + device.getName() + "," + device.getMode() + "," +
                device.getisQuat());
            if(state == 0)
            {
                _currentStates.connectStatus = 1;
                VrsViewer.Instance.SwitchControllerMode(true);
            }
            else
            {
                _currentStates.connectStatus = 0;
            }
        }

        void PointerInEventHandler(object sender, PointerEventArgs e)
        {
            VrsControllerHelper.ControllerRaycastObject = e.target.gameObject;
            
        }

        void PointerOutEventHandler(object sender, PointerEventArgs e)
        {
            VrsControllerHelper.ControllerRaycastObject = null;
            
        }

        
        void Update()
        {
#if UNITY_ANDROID 
            if (!VrsViewer.Instance.IsWinPlatform && !isGamePad)
            {
                
                int noloType = GetNoloType();
                int handType = noloType == (int)CDevice.NOLO_TYPE.LEFT ? (int)InteractionManager.NACTION_HAND_TYPE.HAND_LEFT : (int)InteractionManager.NACTION_HAND_TYPE.HAND_RIGHT;
                bool isConnected = false;
                if (InteractionManager.IsInteractionSDKEnabled())
                {
                    isConnected = InteractionManager.IsSixDofControllerConnected(handType);
                }
                else
                {
                    isConnected = ControllerAndroid.isDeviceConn(noloType);
                }

                if (DebugInEditor)
                {
                    isConnected = true;
                }

                if (_currentStates.connectStatus == 0 && isConnected)
                {
                    VrsViewer.Instance.SwitchControllerMode(true);
                    _currentStates.connectStatus = 1;
                    VrsPlayerCtrl mVrsPlayerCtrl = VrsPlayerCtrl.Instance;
                    if (mVrsPlayerCtrl != null)
                    {
                        
                        mVrsPlayerCtrl.GamepadEnabled = false;
                        
                    }
                }
                else if (_currentStates.connectStatus == 1 && !isConnected)
                {
                    _currentStates.connectStatus = 0;
                }

                
                

               
                if (GetButtonUp(ButtonID.InternalTrigger))
                {
                    VrsSDKApi.Instance.SixDofControllerPrimaryDeviceType = deviceType;
                    Debug.Log("IsPrimaryControllerHand " + deviceType.ToString());
                }

                if (IsConneted())
                {
                    var systemUIControl = VrsViewer.Instance.GetDevice().GetSystemUIControl();
                    processControllerKeyEvent(noloType);
                    float[] poseData = new float[8];
                    if (InteractionManager.IsSixDofControllerConnected(handType))
                    {
                        float[] pose = InteractionManager.GetControllerPose((InteractionManager.NACTION_HAND_TYPE) handType);
                        poseData[1] = pose[4];
                        poseData[2] = pose[5];
                        poseData[3] = pose[6];
                        for (int i = 0; i < 4; i++)
                        {
                            poseData[4 + i] = pose[i];
                        }

                        
                 /*       poseData[4] += 0.2f;*/
                        
                    }
                    else
                    {
                        poseData = ControllerAndroid.getCPoseEvent(noloType, 1);
                    }

                    if(VrsViewer.Instance.SixDofMode == SixDofMode.Head_3Dof_Ctrl_6Dof)
                    {
                        Vector3 HeadPosition = VrsSDKApi.Instance.HeadPosition;
                        
                        poseData[1] = poseData[1] - HeadPosition.x;
                        poseData[2] = poseData[2] - HeadPosition.y;
                        poseData[3] = poseData[3] - HeadPosition.z;
                    }
                    else if (VrsViewer.Instance.SixDofMode == SixDofMode.Head_3Dof_Ctrl_3Dof)
                    {
                        var factor = handType == (int) InteractionManager.NACTION_HAND_TYPE.HAND_LEFT ? -1 : 1;
                        var headPos = VrsViewer.Instance.GetHead().transform.localPosition;
                        var pos = new Vector3(headPos.x + 0.25f * factor, headPos.y - 0.6f, headPos.z + 0.5f);
                        poseData[1] = pos.x;
                        poseData[2] = pos.y;
                        poseData[3] = pos.z;
                    }

                    transform.localPosition = new Vector3(poseData[1], poseData[2], poseData[3]);
                    transform.localRotation = new Quaternion(poseData[4], poseData[5], poseData[6], poseData[7]);
                }
            }
#endif

#if UNITY_STANDALONE_WIN
            if (VrsInstantNativeApi.Inited)
            {
                _prevStates = _currentStates;
                _currentStates = VrsInstantNativeApi.GetControllerStates(deviceType);
                if (isGamePad)
                {
                    
                    if (!IsConneted())
                    {
                        _currentStates = VrsInstantNativeApi.GetControllerStates(VrsInstantNativeApi.ViarusDeviceType.LeftController);
                        if (!IsConneted())
                        {
                            deviceType = VrsInstantNativeApi.ViarusDeviceType.RightController;
                        }
                        else
                        {
                            VrsControllerHelper.HandMode3DOF = VrsControllerHelper.LEFT_HAND_MODE;
                            deviceType = VrsInstantNativeApi.ViarusDeviceType.LeftController;
                            Debug.Log("Current 3dof HandMode is Left !!!");
                        }
                    }
                }
                /*
                if (!IsConneted() && controllerModel != null && controllerModel.gameObject.activeSelf)
                {
                    controllerModel.gameObject.SetActive(false);
                    laserPointer.holder.SetActive(false);
                    if (deviceType == VrsInstantNativeApi.ViarusDeviceType.LeftController)
                    {
                        VrsControllerHelper.IsLeftNoloControllerConnected = false;
                    }
                    else if (deviceType == VrsInstantNativeApi.ViarusDeviceType.RightController)
                    {
                        VrsControllerHelper.IsRightNoloControllerConnected = false;
                    }
                    Debug.Log("controllerModel Dismiss " + deviceType + "," + controllerModel.gameObject.activeSelf);
                }
                else if (IsConneted() && controllerModel != null && !controllerModel.gameObject.activeSelf)
                {
                    controllerModel.gameObject.SetActive(true);
                    laserPointer.holder.SetActive(true);

                    if (VrsControllerHelper.ControllerType == (int)VrsInstantNativeApi.ViarusControllerId.NOLO)
                    {
                        if (deviceType == VrsInstantNativeApi.ViarusDeviceType.LeftController)
                        {
                            VrsControllerHelper.IsLeftNoloControllerConnected = true;
                        }
                        else if (deviceType == VrsInstantNativeApi.ViarusDeviceType.RightController)
                        {
                            VrsControllerHelper.IsRightNoloControllerConnected = true;
                        }
                    }
                    Debug.Log("controllerModel Show " + deviceType);
                }
                else if (!IsConneted() && isGamePad && VrsControllerHelper.Is3DofControllerConnected)
                {
                    VrsControllerHelper.Is3DofControllerConnected = false;
                    Debug.Log("Controller 3dof Dismiss.");
                }
                else if (IsConneted() && isGamePad && !VrsControllerHelper.Is3DofControllerConnected)
                {
                    if (VrsControllerHelper.ControllerType == (int)VrsInstantNativeApi.ViarusControllerId.NORMAL_3DOF)
                    {
                        VrsControllerHelper.Is3DofControllerConnected = true;
                    }
                    Debug.Log("Controller 3dof Show." + VrsControllerHelper.ControllerType);
                }
                *
                if (!isGamePad)
                {
                    VrsInstantNativeApi.Viarus_Pose pose = VrsInstantNativeApi.GetPoseByDeviceType(deviceType);
                    
                    transform.localPosition = pose.position;
                    transform.localRotation = new Quaternion(pose.rotation.x, pose.rotation.y, pose.rotation.z, pose.rotation.w);
                }
            }
#endif
        }

        private int[] lastState;
        private int[] curState;

        private void initState()
        {
            if (lastState == null)
            {
                lastState = new int[256];
                curState = new int[256];
                for (int i = 0; i < 256; i++)
                {
                    curState[i] = -1;
                    lastState[i] = -1;
                }
            }
        }

        private void processControllerKeyEvent(int noloType)
        {
            int handType = noloType == (int)CDevice.NOLO_TYPE.LEFT ? (int)InteractionManager.NACTION_HAND_TYPE.HAND_LEFT : (int)InteractionManager.NACTION_HAND_TYPE.HAND_RIGHT;
            initState();

            _prevStates = _currentStates;
            lastState = curState;
            float[] touchInfo = new float[] {0, CKeyEvent.ACTION_UP, 0, 0}; 
            if (InteractionManager.IsInteractionSDKEnabled())
            {
                curState = InteractionManager.GetKeyAction(handType);
                touchInfo[1] = curState[CKeyEvent.KEYCODE_CONTROLLER_TOUCHPAD_TOUCH];
                if (noloType == (int) CDevice.NOLO_TYPE.LEFT)
                {
                    Vector3 pos = InteractionManager.TouchPadPositionLeft;
                    touchInfo[2] = pos.x;
                    touchInfo[3] = pos.y;
                }
                else
                {
                    Vector3 pos = InteractionManager.TouchPadPositionRight;
                    touchInfo[2] = pos.x;
                    touchInfo[3] = pos.y;
                }
            }
            else
            {
                touchInfo = ControllerAndroid.getTouchEvent(noloType, 1);
                curState = ControllerAndroid.getKeyState(noloType, 1);
            }

            
            int btnViarus = curState[CKeyEvent.KEYCODE_BUTTON_VIARUS];
            int btnStart = curState[CKeyEvent.KEYCODE_BUTTON_START];
            
            int btnSelect = curState[CKeyEvent.KEYCODE_BUTTON_SELECT];
            
            int btnApp = curState[CKeyEvent.KEYCODE_BUTTON_APP];
            
            int btnCenter = curState[CKeyEvent.KEYCODE_DPAD_CENTER];
            
            int btnR1 = curState[CKeyEvent.KEYCODE_BUTTON_R1];
            int btnR2 = curState[CKeyEvent.KEYCODE_BUTTON_R2];
            int btnInternalTrigger = curState[CKeyEvent.KEYCODE_CONTROLLER_TRIGGER_INTERNAL];
            
            
            
            
            
            if (touchInfo[1] == CKeyEvent.ACTION_MOVE)
            {
                _currentStates.touches |= 1 << (int) ButtonID.TrackpadTouch;
                _currentStates.touchpadAxis = new Vector2(touchInfo[2], touchInfo[3]);
            }
            else if (touchInfo[1] == CKeyEvent.ACTION_UP &&
                     ((_currentStates.touches & (1 << (int) ButtonID.TrackpadTouch)) != 0))
            {
                _currentStates.touches = 0;
                _currentStates.touchpadAxis = new Vector2(0, 0);
            }

            if (btnCenter == 0)
            {
                
                _currentStates.buttons |= 1 << (int) ButtonID.TouchPad;
            }
            else if (lastState[CKeyEvent.KEYCODE_DPAD_CENTER] == 0)
            {
                
                _currentStates.buttons -= 1 << (int) ButtonID.TouchPad;
            }

            if (btnApp == 0)
            {
                _currentStates.buttons |= 1 << (int) ButtonID.Menu;
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_APP] == 0)
            {
                _currentStates.buttons -= 1 << (int) ButtonID.Menu;
            }

            if (btnR1 == 0 || btnR2 == 0)
            {
                _currentStates.buttons |= 1 << (int) ButtonID.Trigger;
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_R1] == 0 || lastState[CKeyEvent.KEYCODE_BUTTON_R2] == 0)
            {
                _currentStates.buttons -= 1 << (int) ButtonID.Trigger;
            }

            if (btnInternalTrigger == 0)
            {
                _currentStates.buttons |= 1 << (int) ButtonID.InternalTrigger;
            }
            else if (lastState[CKeyEvent.KEYCODE_CONTROLLER_TRIGGER_INTERNAL] == 0)
            {
                _currentStates.buttons -= 1 << (int) ButtonID.InternalTrigger;
            }

            if (btnSelect == 0)
            {
                _currentStates.buttons |= 1 << (int) ButtonID.Grip;
            }
            else if (lastState[CKeyEvent.KEYCODE_BUTTON_SELECT] == 0)
            {
                _currentStates.buttons -= 1 << (int) ButtonID.Grip;
            }
        }


        public bool IsConneted()
        {
            return _currentStates.connectStatus == 1;
        }

        public bool GetButtonDown(ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int) btn)) != 0 && (_prevStates.buttons & (1 << (int) btn)) == 0;
        }

        public bool GetButtonUp(ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int) btn)) == 0 && (_prevStates.buttons & (1 << (int) btn)) != 0;
        }

        public bool GetButtonPressed(ButtonID btn)
        {
            return (_currentStates.buttons & (1 << (int) btn)) != 0;
        }

        public bool GetTouchPressed(ButtonID btn)
        {
            return (_currentStates.touches & (1 << (int) btn)) != 0;
        }

        public bool GetTouchDown(ButtonID btn)
        {
            return (_currentStates.touches & (1 << (int) btn)) != 0 && (_prevStates.touches & (1 << (int) btn)) == 0;
        }

        public bool GetTouchUp(ButtonID btn)
        {
            return (_currentStates.touches & (1 << (int) btn)) == 0 && (_prevStates.touches & (1 << (int) btn)) != 0;
        }

        public Vector2 GetTouchPosition(ButtonID axisIndex = ButtonID.TrackpadTouch)
        {
            if ((_currentStates.touches & (1 << (int) axisIndex)) != 0)
            {
                return new Vector2(_currentStates.touchpadAxis.x, _currentStates.touchpadAxis.y);
            }

            return Vector2.zero;
        }
    }
}