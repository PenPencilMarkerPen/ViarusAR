












#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using System.Collections.Generic;
using ViarusAxis;


namespace Vrs.Internal
{
    
    public class EditorDevice : BaseARDevice
    {
        
        private static readonly Vector3 neckOffset = new Vector3(0, 0.075f, 0.08f);

        
        private float mouseX = 0;
        private float mouseY = 0;
        private float mouseZ = 0;

        private bool loadConfigData = false;
        private float[] deviceConfigData;
        Quaternion remoteQaut;
        public override void Init()
        {
            Input.gyro.enabled = true;
            
            if (VrsViewer.Instance.RemoteDebug)
            {
                VrsViewer.Instance.InitialRecenter = false;
            }
        }

        public override bool SupportsNativeDistortionCorrection(List<string> diagnostics)
        {
            return false;  
        }

        
        
        public override void SetSplitScreenModeEnabled(bool enabled) { }

        private Quaternion initialRotation = Quaternion.identity;

        public override void UpdateState()
        {
            Quaternion rot = Quaternion.identity;

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                mouseX += Input.GetAxis("Mouse X") * 5;
                if (mouseX <= -180)
                {
                    mouseX += 360;
                }
                else if (mouseX > 180)
                {
                    mouseX -= 360;
                }
                mouseY -= Input.GetAxis("Mouse Y") * 2.4f;
                mouseY = Mathf.Clamp(mouseY, -85, 85);
            }
            else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                mouseZ += Input.GetAxis("Mouse X") * 5;
                mouseZ = Mathf.Clamp(mouseZ, -85, 85);
            }

            bool IsNeedUpdatePose = false;
            if (Application.isEditor && VrsViewer.Instance.RemoteDebug)
            {
                if (remoteQaut.w != 0)
                {
                    rot = remoteQaut;
                    IsNeedUpdatePose = true;
                }
            }

            if (mouseX != 0 || mouseY != 0 || mouseZ != 0)
            {
                rot = Quaternion.Euler(mouseY, mouseX, mouseZ);
                IsNeedUpdatePose = true;
            }

            if (IsNeedUpdatePose)
            {
                headPose.Set(Vector3.zero, rot);
            }

#if UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
            
            if (VrsInstantNativeApi.Inited)
            {
                VrsInstantNativeApi.Viarus_Pose pose = VrsInstantNativeApi.GetPoseByDeviceType(VrsInstantNativeApi.ViarusDeviceType.Hmd);
                if (pose.rotation.w == 0)
                {
                    pose.rotation.w = 1;
                }
                this.headPose.Set(pose.position, new Quaternion(pose.rotation.x, pose.rotation.y, -pose.rotation.z, -pose.rotation.w));
            }
#endif
            isHeadPoseUpdated = true;
        }

        public override void PostRender(RenderTexture stereoScreen)
        {
            
        }

        public override void UpdateScreenData()
        {
            
            string deviceConfigInfo = "ar device config parameter : ";
            if (deviceConfigData != null)
            {
                for (int i = 0; i < deviceConfigData.Length; i++)
                {
                    deviceConfigInfo += deviceConfigData[i];
                    if (i < deviceConfigData.Length - 1)
                    {
                        deviceConfigInfo += ",";
                    }
                }
                Debug.Log(deviceConfigInfo);
            }

            Profile = VrsProfile.GetKnownProfile(VrsViewer.Instance.ScreenSize, VrsViewer.Instance.ViewerType);
            if (loadConfigData && deviceConfigData != null)
            {
                Profile.screen.width = deviceConfigData[12];
                Profile.screen.height = deviceConfigData[13];
                Profile.viewer = new VrsProfile.Viewer
                {
                    lenses = {
                      separation = deviceConfigData[0],
                      offset = deviceConfigData[1],
                      screenDistance = deviceConfigData[2],
                      alignment = VrsProfile.Lenses.AlignBottom,
                    },
                    maxFOV = {
                              outer = deviceConfigData[3],
                              inner = deviceConfigData[4],
                              upper = deviceConfigData[5],
                              lower = deviceConfigData[6]
                            },
                    distortion = {
                          Coef = new [] { deviceConfigData[7], deviceConfigData[8] },
                        },
                    inverse = VrsProfile.ApproximateInverse(new[] { deviceConfigData[7], deviceConfigData[8] })
                };

            }

            if(userIpd > 0)
            {
                Profile.viewer.lenses.separation = userIpd;
            }

            ComputeEyesFromProfile(1, 2000);
            profileChanged = true;
            Debug.Log("UpdateScreenData=" + Profile.viewer.lenses.separation);
        }

        public override void Recenter()
        {
            mouseX = mouseZ = 0;  
        }
        public override bool GazeApi(GazeTag tag, string param)
        {
            return true;
        }

        public override void SetCameraNearFar(float near, float far)
        {
            Debug.Log("EditorDevice.SetCameraNearFar : " + near + "," + far);
        }

        private bool isHeadPoseUpdated = false;
        private float userIpd = -1;
        public override void SetIpd(float ipd)
        {
            userIpd = ipd;
        }

        public override bool IsHeadPoseUpdated()
        {
            return isHeadPoseUpdated;
        }
    }
}


#endif
