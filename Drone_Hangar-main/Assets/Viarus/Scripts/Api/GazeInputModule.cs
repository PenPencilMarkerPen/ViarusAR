





















using ViarusAxis;
using ViarusTask;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


















namespace Vrs.Internal
{
    [AddComponentMenu("VRS/GazeInputModule")]
    public class GazeInputModule : BaseInputModule
    {
        public static List<VrsUIPointer> pointers = new List<VrsUIPointer>();

        public static void AddPoint(VrsUIPointer point)
        {
            if (!pointers.Contains(point))
                pointers.Add(point);
        }

        public static void RemovePoint(VrsUIPointer point)
        {
            if (pointers.Contains(point))
                pointers.Remove(point);
        }

        private static Vector3 raycastResultPosition = Vector3.zero;

        public static Vector3 GetRaycastResultPosition()
        {
            return raycastResultPosition;
        }

        
        
        
        [Tooltip("Whether gaze input is active in Split Mode only (true), or all the time (false).")]
        private bool splitModeOnly = false;

        
        public static IVrsGazePointer gazePointer;

        private PointerEventData pointerData;
        private Vector2 lastHeadPose;

        
        private bool isActive = false;

        
        
        private const float clickTime = 0.1f; 

        private Vector2 screenCenterVec = Vector2.zero;

        bool isShowGaze = true;

        
        public override bool ShouldActivateModule()
        {
            bool activeState = base.ShouldActivateModule();
            
            activeState = activeState && (VrsViewer.Instance.SplitScreenModeEnabled || !splitModeOnly);

            if (activeState != isActive)
            {
                isActive = activeState;

                
                if (gazePointer != null)
                {
                    if (isActive)
                    {
                        gazePointer.OnGazeEnabled();
                    }
                }
            }

            return activeState;
        }

        
        public override void DeactivateModule()
        {
            Debug.Log("DeactivateModule");
            DisableGazePointer();
            base.DeactivateModule();
            if (pointerData != null)
            {
                HandlePendingClick();
                HandlePointerExitAndEnter(pointerData, null);
                pointerData = null;
            }

            eventSystem.SetSelectedGameObject(null, pointerData);
            Debug.Log("DeactivateModule");
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                VrsViewer.Instance.Triggered = false;
                if (pointerData != null && pointerData.selectedObject != null)
                {
                    HandlePointerExitAndEnter(pointerData, null);
                    eventSystem.SetSelectedGameObject(null, pointerData);
                    pointerData.Reset();
                }
            }
        }

        public override bool IsPointerOverGameObject(int pointerId)
        {
            return pointerData != null && pointerData.pointerEnter != null;
        }

        public override void Process()
        {
            
            if (TouchScreenKeyboard.visible)
            {
                VrsViewer.Instance.DismissReticle();

                VrsViewer.Instance.Triggered = false;
                pointerData.eligibleForClick = false;
                return;
            }

#if UNITY_ANDROID && !ANDROID_REMOTE_NRR
            if ((VrsPlayerCtrl.Instance.GamepadEnabled && ViarusTaskApi.IsQuatConn()) || ControllerAndroid.IsNoloConn())
#elif UNITY_STANDALONE_WIN || ANDROID_REMOTE_NRR
            if (VrsControllerHelper.IsLeftNoloControllerConnected || VrsControllerHelper.IsRightNoloControllerConnected)
            {
                CastRayFromController(true);
                if (isShowGaze)
                {
                    VrsViewer.Instance.GazeApi(GazeTag.Hide, "");
                    isShowGaze = false;
                }
            }
            else if (VrsControllerHelper.Is3DofControllerConnected)
#else
            if(true)
#endif
            {
                
                CastRayFromGamepad();
                if (isShowGaze)
                {
                    VrsViewer.Instance.SwitchControllerMode(true);
                    isShowGaze = false;
                }
            }
            else
            {
                if (!isShowGaze)
                {
                    VrsViewer.Instance.SwitchControllerMode(false);
                    isShowGaze = true;
                    pointerData.pointerPress = null;
                    HandlePointerExitAndEnter(pointerData, null);
                }

                
                GameObject gazeObjectPrevious = GetCurrentGameObject();
                CastRayFromGaze();
                UpdateCurrentObject();
                UpdateReticle(gazeObjectPrevious);
            }


            if (!pointerData.eligibleForClick && VrsViewer.Instance.Triggered)
            {
                
                HandleTrigger();
                VrsViewer.Instance.Triggered = false;
            }
            else if (!VrsViewer.Instance.Triggered)
            {
                
                HandlePendingClick();
            }
            else if (pointerData.eligibleForClick && VrsViewer.Instance.Triggered)
            {
                VrsViewer.Instance.Triggered = false;
            }
            SetRaycastResultPosition();
        }

        
        private void CastRayFromController(bool isNolo)
        {
            if (pointerData == null)
            {
                pointerData = new PointerEventData(eventSystem);
            }

            
            pointerData.Reset();
            GameObject gameObject = isNolo ? VrsControllerHelper.ControllerRaycastObject : null;
            if (gameObject != null)
            {
                pointerData.pointerPress = gameObject;
                RaycastResult raycastResult = new RaycastResult();
                raycastResult.gameObject = gameObject;
                pointerData.pointerCurrentRaycast = raycastResult;
                HandlePointerExitAndEnter(pointerData, gameObject);
            }
            else
            {
                RaycastResult raycastResult = new RaycastResult();
                raycastResult.gameObject = null;
                pointerData.pointerCurrentRaycast = raycastResult;
                pointerData.pointerPress = null;
                HandlePointerExitAndEnter(pointerData, null);
            }
        }


        private void CastRayFromGamepad()
        {
            if (pointerData == null)
            {
                pointerData = new PointerEventData(eventSystem);
            }

            
            pointerData.Reset();
            bool IsUICanvasObject = VrsControllerHelper.ControllerRaycastObject != null &&
                                    VrsControllerHelper.ControllerRaycastObject.GetComponent<VrsUICanvas>() != null;
            if (IsUICanvasObject)
            {
                for (int i = 0; i < pointers.Count; i++)
                {
                    VrsUIPointer pointer = pointers[i];
                    if (pointer.gameObject.activeInHierarchy && pointer.enabled && pointer.IsShow())
                    {
                        var raycastResult = new RaycastResult();
                        raycastResult.worldPosition = pointer.transform.position;
                        raycastResult.worldNormal = pointer.transform.forward;
                        
                        pointerData.pointerCurrentRaycast = raycastResult;
                        eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
                        if (m_RaycastResultCache.Count > 1)
                        {
                            if (m_RaycastResultCache[0].gameObject.GetComponent<Canvas>())
                            {
                                var raycastResultCache = m_RaycastResultCache[0];
                                m_RaycastResultCache.RemoveAt(0);
                                m_RaycastResultCache.Add(raycastResultCache);
                            }
                        }

                        pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
                        if (m_RaycastResultCache.Count > 0)
                        {
                            HandlePointerExitAndEnter(pointerData, pointerData.pointerCurrentRaycast.gameObject);
                        }
                        else
                        {
                            HandlePointerExitAndEnter(pointerData, null);
                        }

                        m_RaycastResultCache.Clear();
                    }
                }
            }
            else
            {
                pointerData.pointerPress = VrsControllerHelper.ControllerRaycastObject;
                RaycastResult raycastResult = new RaycastResult();
                raycastResult.gameObject = VrsControllerHelper.ControllerRaycastObject;
                pointerData.pointerCurrentRaycast = raycastResult;
                HandlePointerExitAndEnter(pointerData, VrsControllerHelper.ControllerRaycastObject);
            }
            if (pointerData.pointerCurrentRaycast.gameObject == null && eventSystem.currentSelectedGameObject != null)
            {
                Debug.LogError("Clear Seleted GameObject-Controller=>" + eventSystem.currentSelectedGameObject.name);
                eventSystem.SetSelectedGameObject(null);
            }
        }

        private bool isSpecial;

        private void CastRayFromGaze()
        {
            Vector2 headPose =
                NormalizedCartesianToSpherical(VrsViewer.Instance.HeadPose.Orientation * Vector3.forward);

            if (pointerData == null)
            {
                pointerData = new PointerEventData(eventSystem);
                lastHeadPose = headPose;
            }

            Vector2 diff = headPose - lastHeadPose;

            if (screenCenterVec.x == 0)
            {
                screenCenterVec = new Vector2(0.5f * Screen.width, 0.5f * Screen.height);
            }

            
            pointerData.Reset();
            var raycastResult = new RaycastResult();
            raycastResult.Clear();
            Transform mTrans = VrsViewer.Instance.GetHead().GetTransform();
            raycastResult.worldPosition = mTrans.position;
            raycastResult.worldNormal = mTrans.forward;
            pointerData.pointerCurrentRaycast = raycastResult;
            pointerData.position = new Vector2(0.5f * Screen.width, 0.5f * Screen.height);
            eventSystem.RaycastAll(pointerData, m_RaycastResultCache);
            foreach (var mRaycastResult in m_RaycastResultCache)
            {
                if (mRaycastResult.gameObject.layer == 8)
                {
                    pointerData.pointerCurrentRaycast = mRaycastResult;
                    isSpecial = true;
                    break;
                }
            }

            if (!isSpecial)
            {
                pointerData.pointerCurrentRaycast = FindFirstRaycast(m_RaycastResultCache);
                foreach (RaycastResult mRaycastResult in m_RaycastResultCache)
                {
                    bool IsUICanvasObject = mRaycastResult.gameObject != null &&
                                            mRaycastResult.gameObject.GetComponent<VrsUICanvas>() != null;
                    if (!IsUICanvasObject)
                    {
                        pointerData.pointerCurrentRaycast = mRaycastResult;
                        break;
                    }
                }
            }

            isSpecial = false;
            m_RaycastResultCache.Clear();
            pointerData.delta = diff;
            lastHeadPose = headPose;
            if (pointerData.pointerCurrentRaycast.gameObject == null && eventSystem.currentSelectedGameObject != null)
            {
                Debug.LogError("Clear Seleted GameObject-Gaze=>" + eventSystem.currentSelectedGameObject.name);
                eventSystem.SetSelectedGameObject(null);
            }
        }

        private void SetRaycastResultPosition()
        {
            if (pointerData != null && pointerData.pointerCurrentRaycast.gameObject)
            {
                raycastResultPosition = pointerData.pointerCurrentRaycast.worldPosition;
            }
            else
            {
                raycastResultPosition = Vector3.zero;
            }
        }

        private void UpdateCurrentObject()
        {
            if (pointerData == null)
            {
                return;
            }

            
            var go = pointerData.pointerCurrentRaycast.gameObject;
            HandlePointerExitAndEnter(pointerData, go);
        }


        float lastGazeZ = 0;

        void UpdateReticle(GameObject previousGazedObject)
        {
            if (pointerData == null)
            {
                return;
            }

            Camera camera = pointerData.enterEventCamera; 
            Vector3 intersectionPosition = GetIntersectionPosition();
            GameObject gazeObject = GetCurrentGameObject(); 

            if (gazeObject != null && VrsOverrideSettings.OnGazeEvent != null)
            {
                VrsOverrideSettings.OnGazeEvent(gazeObject);
            }

            float gazeZ = VrsGlobal.defaultGazeDistance;
            if (gazeObject != null)
            {
                gazeZ = intersectionPosition
                    .z; 
            }

            if (lastGazeZ != gazeZ)
            {
                lastGazeZ = gazeZ;
                
                VrsViewer.Instance.GazeApi(Vrs.Internal.GazeTag.Set_Distance, (-1 * gazeZ).ToString());
            }

            

            
            VrsGlobal.focusObjectDistance = (int) (Mathf.Abs(gazeZ) * 100) / 100.0f;

            if (gazePointer == null)
            {
                if (gazeObject != null)
                {
                    IVrsGazeResponder mGazeResponder = gazeObject.GetComponent<IVrsGazeResponder>();
                    if (mGazeResponder != null)
                    {
                        mGazeResponder.OnUpdateIntersectionPosition(intersectionPosition);
                    }
                }
                else
                {
                    
                }

                return;
            }

            bool isInteractive = pointerData.pointerPress == gazeObject ||
                                 ExecuteEvents.GetEventHandler<ISelectHandler>(gazeObject) != null;

            if (gazeObject != null && gazeObject != previousGazedObject)
            {
                
                if (previousGazedObject != null)
                {
                    
                }
            }
            else if (gazeObject == null && previousGazedObject != null)
            {
                
            }

            if (gazeObject == previousGazedObject)
            {
                if (gazeObject != null && gazePointer != null)
                {
                    gazePointer.OnGazeStay(camera, gazeObject, intersectionPosition, isInteractive);
                    IVrsGazeResponder mGazeResponder = gazeObject.GetComponent<IVrsGazeResponder>();
                    if (mGazeResponder != null)
                    {
                        mGazeResponder.OnUpdateIntersectionPosition(intersectionPosition);
                    }
                }
            }
            else
            {
                if (previousGazedObject != null && gazePointer != null)
                {
                    gazePointer.OnGazeExit(camera, previousGazedObject);
                    if (VrsViewer.Instance != null)
                    {
                        if (VrsViewer.Instance.HeadControl == HeadControl.Hover)
                        {
                            if (ViarusHMDControl.baseEventData != null)
                            {
                                ViarusHMDControl.baseEventData = null;
                                ViarusHMDControl.eventGameObject = null;
                            }
                        }
                    }
                }

                if (gazeObject != null && gazePointer != null)
                {
                    gazePointer.OnGazeStart(camera, gazeObject, intersectionPosition, isInteractive);
                    if (VrsViewer.Instance != null)
                    {
                        if (VrsViewer.Instance.HeadControl == HeadControl.Hover)
                        {
                            ViarusHMDControl.baseEventData = pointerData;
                        }
                    }
                }
            }
        }

        private void HandleDrag()
        {
            bool moving = pointerData.IsPointerMoving();

            if (moving && pointerData.pointerDrag != null && !pointerData.dragging)
            {
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData,
                    ExecuteEvents.beginDragHandler);
                pointerData.dragging = true;
            }

            
            if (pointerData.dragging && moving && pointerData.pointerDrag != null)
            {
                
                
                if (pointerData.pointerPress != pointerData.pointerDrag)
                {
                    ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);

                    pointerData.eligibleForClick = false;
                    pointerData.pointerPress = null;
                    pointerData.rawPointerPress = null;
                }

                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.dragHandler);
            }
        }

        private void HandlePendingClick()
        {
            if (!pointerData.eligibleForClick && !pointerData.dragging)
            {
                return;
            }

            if (gazePointer != null)
            {
                Camera camera = pointerData.enterEventCamera;
                gazePointer.OnGazeTriggerEnd(camera);
            }

            var go = pointerData.pointerCurrentRaycast.gameObject;

            
            ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerUpHandler);
            
            if (pointerData.eligibleForClick)
            {
                if (VrsViewer.Instance != null)
                {
                    if (VrsViewer.Instance.HeadControl != HeadControl.Hover ||
                        (VrsPlayerCtrl.Instance.IsQuatConn() || ControllerAndroid.IsNoloConn()))
                    {
                        ExecuteEvents.Execute(pointerData.pointerPress, pointerData, ExecuteEvents.pointerClickHandler);
                    }
                }
            }
            else if (pointerData.dragging)
            {
                ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.dropHandler);
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.endDragHandler);
            }

            
            pointerData.pointerPress = null;
            pointerData.rawPointerPress = null;
            pointerData.eligibleForClick = false;
            pointerData.clickCount = 0;
            pointerData.clickTime = 0;
            pointerData.pointerDrag = null;
            pointerData.dragging = false;
        }

        private void HandleTrigger()
        {
            var go = pointerData.pointerCurrentRaycast.gameObject;

            
            pointerData.pressPosition = pointerData.position;
            pointerData.pointerPressRaycast = pointerData.pointerCurrentRaycast;
            pointerData.pointerPress =
                ExecuteEvents.ExecuteHierarchy(go, pointerData, ExecuteEvents.pointerDownHandler)
                ?? ExecuteEvents.GetEventHandler<IPointerClickHandler>(go);

            
            pointerData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(go);
            if (pointerData.pointerDrag != null)
            {
                ExecuteEvents.Execute(pointerData.pointerDrag, pointerData, ExecuteEvents.initializePotentialDrag);
            }

            
            pointerData.rawPointerPress = go;
            pointerData.eligibleForClick = true;
            pointerData.delta = Vector2.zero;
            pointerData.dragging = false;
            pointerData.useDragThreshold = true;
            pointerData.clickCount = 1;
            pointerData.clickTime = Time.unscaledTime;

            if (gazePointer != null)
            {
                gazePointer.OnGazeTriggerStart(pointerData.enterEventCamera);
            }
        }

        private Vector2 NormalizedCartesianToSpherical(Vector3 cartCoords)
        {
            cartCoords.Normalize();
            if (cartCoords.x == 0)
                cartCoords.x = Mathf.Epsilon;
            float outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
            if (cartCoords.x < 0)
                outPolar += Mathf.PI;
            float outElevation = Mathf.Asin(cartCoords.y);
            return new Vector2(outPolar, outElevation);
        }

        GameObject GetCurrentGameObject()
        {
            if (pointerData != null && pointerData.enterEventCamera != null)
            {
                return pointerData.pointerCurrentRaycast.gameObject;
            }

            return null;
        }

        Vector3 GetIntersectionPosition()
        {
            
            Camera cam = pointerData.enterEventCamera;
            if (cam == null)
            {
                return Vector3.zero;
            }

            float intersectionDistance = pointerData.pointerCurrentRaycast.distance + cam.nearClipPlane;
            Vector3 intersectionPosition = cam.transform.position + cam.transform.forward * intersectionDistance;

            return intersectionPosition;
        }

        void DisableGazePointer()
        {
            if (gazePointer == null)
            {
                return;
            }

            GameObject currentGameObject = GetCurrentGameObject();
            if (currentGameObject)
            {
                Camera camera = pointerData.enterEventCamera;
                gazePointer.OnGazeExit(camera, currentGameObject);
                if (VrsViewer.Instance != null)
                {
                    if (VrsViewer.Instance.HeadControl == HeadControl.Hover)
                    {
                        if (ViarusHMDControl.baseEventData != null)
                            ViarusHMDControl.baseEventData = null;
                    }
                }
            }

            gazePointer.OnGazeDisabled();
        }
    }
}