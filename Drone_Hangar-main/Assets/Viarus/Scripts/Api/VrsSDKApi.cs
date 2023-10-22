using System;
using ViarusTask;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Vrs.Internal
{
    
    
    
    public class VrsSDKApi
    {
        private static object syncRoot = new object();

        private static VrsSDKApi _instance = null;

        public static VrsSDKApi Instance
        {
            get
            {
                if (_instance == null) 
                {
                    lock (syncRoot) 
                    {
                        if (_instance == null) 
                        {
                            _instance = new VrsSDKApi();
                        }
                    }
                }

                return _instance;
            }
        }

        private VrsSDKApi()
        {
            IsIVrsMode = false;
        }

        
        
        
        public bool IsIVrsMode { set; get; }

        
        public delegate void ControllerStatusChange(ViarusTask.InteractionManager.NACTION_HAND_TYPE handType,
            bool isConnected, bool isSixDofController);

        public event ControllerStatusChange ControllerStatusChangeEvent;

        public void ExecuteControllerStatusChangeEvent(ViarusTask.InteractionManager.NACTION_HAND_TYPE handType,
            bool isConnected, bool isSixDofController)
        {
            Debug.Log("handtype=" + handType + "," + "isConnected=" + isConnected + "," + "isSixDofController=" + isSixDofController);
            if (ControllerStatusChangeEvent != null)
            {
                ControllerStatusChangeEvent(handType, isConnected, isSixDofController);
            }

            if (isConnected)
            {
                var controllerTipState = VrsViewer.Instance.GetDevice().GetControllerTipState();
                Debug.Log("ControllerTipState:" + controllerTipState);
                if (controllerTipState == 0)
                {
                    VrsViewer.Instance.GetDevice().SetControllerTipState(1);
                }
            }
        }

        
        
        
        VrsInstantNativeApi.ViarusDeviceType sixDofControllerPrimaryDeviceType;

        
        
        
        public VrsInstantNativeApi.ViarusDeviceType SixDofControllerPrimaryDeviceType
        {
            set
            {
                sixDofControllerPrimaryDeviceType = value;
                if (VrsViewer.Instance != null)
                {
                    VrsViewer.Instance.GetDevice()
                        .SetSixDofControllerPrimaryDeviceType(sixDofControllerPrimaryDeviceType);
                }
            }
            get { return sixDofControllerPrimaryDeviceType; }
        }

        
        
        
        public Vector3 HeadPosition { set; get; }

        private Dictionary<string, Sprite> Cach3DofSpriteDict = new Dictionary<string, Sprite>();
        public Dictionary<string, Sprite> Cach6DofSpriteDict = new Dictionary<string, Sprite>();
        public bool Is3DofSpriteFirstLoad { set; get; }
        public bool Is6DofSpriteFirstLoad { set; get; }
        public string Last3DofModelPath { set; get; }
        public string Last6DofModelPath { set; get; }

        public void AddSprite(string name, Sprite sprite)
        {
            if (InteractionManager.GetControllerModeType() == InteractionManager.NACTION_CONTROLLER_TYPE.CONTROL_3DOF)
            {
                if (Cach3DofSpriteDict.ContainsKey(name))
                {
                    Cach3DofSpriteDict[name] = sprite;
                }
                else
                {
                    Cach3DofSpriteDict.Add(name, sprite);
                }
            }

            if (InteractionManager.GetControllerModeType() == InteractionManager.NACTION_CONTROLLER_TYPE.CONTROL_6DOF)
            {
                if (Cach6DofSpriteDict.ContainsKey(name))
                {
                    Cach6DofSpriteDict[name] = sprite;
                }
                else
                {
                    Cach6DofSpriteDict.Add(name, sprite);
                }
            }
        }

        public Sprite GetSprite(string name)
        {
            if (InteractionManager.GetControllerModeType() == InteractionManager.NACTION_CONTROLLER_TYPE.CONTROL_3DOF)
            {
                if (Cach3DofSpriteDict.ContainsKey(name)) return Cach3DofSpriteDict[name];
            }

            if (InteractionManager.GetControllerModeType() == InteractionManager.NACTION_CONTROLLER_TYPE.CONTROL_6DOF)
            {
                if (Cach6DofSpriteDict.ContainsKey(name)) return Cach6DofSpriteDict[name];
            }

            return null;
        }

        public void ClearCachSpriteDict()
        {
            if (InteractionManager.GetControllerModeType() == InteractionManager.NACTION_CONTROLLER_TYPE.CONTROL_3DOF)
            {
                Cach3DofSpriteDict.Clear();
            }

            if (InteractionManager.GetControllerModeType() == InteractionManager.NACTION_CONTROLLER_TYPE.CONTROL_6DOF)
            {
                Cach6DofSpriteDict.Clear();
            }
        }

        public void Destroy()
        {
        }

        
        
        
        public bool IsSptMultiThreadedRendering { set; get; }

        
        
        
        
        public bool IsSptControllerTipUI()
        {
            return InteractionManager.IsSptControllerTipUI();
        }

        public List<InteractionManager.ControllerKeyInfo> GetControllerKeyInfoList()
        {
            return InteractionManager.GetControllerConfig().KeyInfoList;
        }

        
        
        
        private InteractionManager.TipLanguage tipLanguage;

        public InteractionManager.TipLanguage GetTipLanguage()
        {
            if (Application.systemLanguage == SystemLanguage.Chinese ||
                Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                Application.systemLanguage == SystemLanguage.ChineseTraditional)
            {
                tipLanguage = InteractionManager.TipLanguage.ZH;
            }
            else if (Application.systemLanguage == SystemLanguage.English)
            {
                tipLanguage = InteractionManager.TipLanguage.EN;
            }
            else
            {
                tipLanguage = InteractionManager.TipLanguage.DEFAULT;
            }

            return tipLanguage;
        }

        public string assetPath = "Assets/Viarus/Resources/Config/";

#if UNITY_EDITOR
        public SettingsAssetConfig GetSettingsAssetConfig()
        {
            var assetpath = assetPath + "SettingsAssetConfig.asset";
            SettingsAssetConfig asset;
            if (System.IO.File.Exists(assetpath))
            {
                asset = UnityEditor.AssetDatabase.LoadAssetAtPath<SettingsAssetConfig>(assetpath);
            }
            else
            {
                asset = new SettingsAssetConfig();
                asset.mSixDofMode = SixDofMode.Head_3Dof_Ctrl_6Dof;
                asset.mSleepTimeoutMode = SleepTimeoutMode.NEVER_SLEEP;
                asset.mHeadControl = HeadControl.GazeApplication;
                asset.mTextureQuality = TextureQuality.Best;
                asset.mTextureMSAA = TextureMSAA.MSAA_2X;
                UnityEditor.AssetDatabase.CreateAsset(asset, assetpath);
            }

            return asset;
        }
#endif



        public bool IsSptEyeLocalRp { get; set; }
        public float[] LeftEyeLocalRotation = new float[9];
        public float[] LeftEyeLocalPosition = new float[3];
        public float[] RightEyeLocalRotation = new float[9];
        public float[] RightEyeLocalPosition = new float[3];

    }
}