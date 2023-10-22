












using ViarusAxis;
using UnityEngine;


















namespace Vrs.Internal
{
    [AddComponentMenu("VRS/VrsHead")]
    public class VrsHead : MonoBehaviour
    {
        public Vector3 BasePosition { set; get; }

        
        
        
        private bool trackRotation = true;

        
        
        
        private bool trackPosition = false;

        
        
        
        
        public void SetTrackPosition(bool b)
        {
            Debug.Log("VrsHead.SetTrackPosition." + b);
            trackPosition = b;
        }

        
        
        
        
        public void SetTrackRotation(bool b)
        {
            trackRotation = b;
        }

        public bool IsTrackRotation()
        {
            return trackRotation;
        }

        public bool IsTrackPosition()
        {
            return trackPosition;
        }

        public void Update3rdPartyPosition(Vector3 pos)
        {
            if (VrsViewer.Instance.UseThirdPartyPosition)
            {
                mTransform.position = pos;
            }
        }

        void Awake()
        {
            VrsViewer.Create();
        }

        protected Transform mTransform;

        public Transform GetTransform()
        {
            return mTransform;
        }

        void Start()
        {
            mTransform = this.transform;
        }

        
        void LateUpdate()
        {
            VrsViewer.Instance.UpdateHeadPose();
            UpdateHead();
        }

        
        private float initEulerYAngle = float.MaxValue;

        private float totalTime = 0;
        private bool triggerLerp = false;
        
        private bool hasResetTracker = true;
        private float moveSpeed = 2.0f;
        
        private void UpdateHead()
        {
            if (VrsGlobal.hasInfinityARSDK)
            {
                trackRotation = false;
                trackPosition = false;
            }

            if (VrsViewer.Instance.GetViarusService() != null && VrsViewer.Instance.GetViarusService().IsMarkerRecognizeRunning)
            {
                if (VrsGlobal.isMarkerVisible)
                {
                    
                    trackRotation = false;
                    trackPosition = false;
                    return;
                }
                else
                {
                    
                    trackRotation = true;
                    trackPosition = false;
                }
            }

            if (trackRotation)
            {
                float[] eulerRange = VrsViewer.Instance.GetHeadEulerAnglesRange();
               Quaternion rot = VrsViewer.Instance.HeadPose.Orientation;
                /*
                if (rot.eulerAngles.y != 0 && initEulerYAngle == float.MaxValue)
                {
                    initEulerYAngle = rot.eulerAngles.y;
                    if (float.IsNaN(initEulerYAngle))
                    {
                        Debug.Log("DATA IS ABNORMAL--------------------------->>>>>>>>>");
                        initEulerYAngle = float.MaxValue;
                    }
                }

                if (initEulerYAngle != float.MaxValue && VrsViewer.Instance.InitialRecenter && !triggerLerp
                     && (Mathf.Abs(initEulerYAngle) <= 345 && Mathf.Abs(initEulerYAngle) >= 15))
                {
                    
                    triggerLerp = true;
                    moveSpeed += (Mathf.Abs(initEulerYAngle) > 60 ? 0.3f : Mathf.Abs(initEulerYAngle) > 90 ? 0.5f : 0);
                    Debug.Log("triggerLerp.yaw=" + initEulerYAngle + ", sp=" + moveSpeed);
                }

                if (triggerLerp)
                {
                    totalTime += Time.deltaTime * 2.10f;
                    if (totalTime > 1)
                    {
                        if (!hasResetTracker)
                        {
                            
                            rot.eulerAngles = new Vector3(rot.eulerAngles.x, rot.eulerAngles.y - initEulerYAngle, rot.eulerAngles.z);
                            hasResetTracker = true;
                            VrsViewer.Instance.ResetHeadTrackerFromAndroid();
                        }
                    }
                    else
                    {
                        rot.eulerAngles = new Vector3(rot.eulerAngles.x, Mathf.LerpAngle(initEulerYAngle, 0, totalTime), rot.eulerAngles.z);
                    }

                }
                */

                if(!hasResetTracker)
                {
                    hasResetTracker = true;
                    VrsViewer.Instance.Recenter();
                }

                Vector3 eulerAngles = rot.eulerAngles;
                if (eulerRange == null ||
                        (
                        
                        (eulerRange != null && (eulerAngles[1] >= eulerRange[0] || eulerAngles[1] < eulerRange[1]) &&
                        
                        (eulerAngles[0] >= eulerRange[2] || eulerAngles[0] < eulerRange[3]))
                        )
                    )
                {
                    mTransform.localRotation = rot;
                }

            }

#if UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
            Vector3 pos = VrsViewer.Instance.HeadPose.Position;
            if (pos.x !=0 && pos.y !=0 && pos.z != 0)
            {
                mTransform.localPosition = BasePosition + pos;
            }
#elif UNITY_ANDROID
            if (trackPosition)
            {
                Vector3 pos = VrsViewer.Instance.HeadPose.Position;
                mTransform.localPosition = BasePosition + pos;
                if (VrsPlayerCtrl.Instance != null)
                {
                    VrsPlayerCtrl.Instance.HeadPosition = mTransform.position;
                }
            }
#endif
        }

        public void ResetInitEulerYAngle()
        {
            initEulerYAngle = 0;
        }

#if UNITY_EDITOR
        private void Update()
        {
            Vector3 start = transform.position;
            Vector3 vector = transform.TransformDirection(Vector3.forward);
            UnityEngine.Debug.DrawRay(start, vector * 20, Color.red);
        }
#endif
    }
}