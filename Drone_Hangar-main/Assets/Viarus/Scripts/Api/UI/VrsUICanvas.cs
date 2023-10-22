using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Vrs.Internal
{
    public class VrsUICanvas : MonoBehaviour
    {
        protected BoxCollider canvasBoxCollider;
        protected Rigidbody canvasRigidBody;

        private int m_CanvasLength;
        private void OnEnable()
        {
            SetupCanvas();
        }
        
        void SetupCanvas()
        {
            var canvas = GetComponent<Canvas>();
            if (!canvas || canvas.renderMode != RenderMode.WorldSpace)
            {
                return;
            }

            var canvasRectTransform = canvas.GetComponent<RectTransform>();
            var canvasSize = canvasRectTransform.sizeDelta;

            var defaultRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
            var customRaycaster = canvas.gameObject.GetComponent<VrsUIGraphicRaycaster>();


            if (!customRaycaster)
            {
                customRaycaster = canvas.gameObject.AddComponent<VrsUIGraphicRaycaster>();
            }

            if (defaultRaycaster && defaultRaycaster.enabled)
            {
                customRaycaster.ignoreReversedGraphics = defaultRaycaster.ignoreReversedGraphics;
                customRaycaster.blockingObjects = defaultRaycaster.blockingObjects;
                defaultRaycaster.enabled = false;
            }
            if (!canvas.gameObject.GetComponent<BoxCollider>())
            {
                float zSize = 0.1f;
                float zScale = zSize / canvasRectTransform.localScale.z;

                canvasBoxCollider = canvas.gameObject.AddComponent<BoxCollider>();
                canvasBoxCollider.size = new Vector3(canvasSize.x, canvasSize.y, zScale);
                canvasBoxCollider.isTrigger = true;
            }

            if (!canvas.gameObject.GetComponent<Rigidbody>())
            {
                canvasRigidBody = canvas.gameObject.AddComponent<Rigidbody>();
                canvasRigidBody.isKinematic = true;
            }
        }

        private void Update()
        { 
            
#if !UNITY_2019_3_OR_NEWER
            Canvas[] canvasArray = GameObject.FindObjectsOfType<Canvas>();
            if (canvasArray.Length != m_CanvasLength)
            {
                for (int i = 0; i < canvasArray.Length; i++)
                {
                    VrsUIGraphicRaycaster graphicRaycaster = canvasArray[i].GetComponent<VrsUIGraphicRaycaster>();
                    if (graphicRaycaster==null)
                    {
                        canvasArray[i].gameObject.AddComponent<VrsUIGraphicRaycaster>();
                    }
                }

                m_CanvasLength = canvasArray.Length;
            }
#endif
        }

        void RemoveCanvas()
        {
            var canvas = GetComponent<Canvas>();

            if (!canvas)
            {
                return;
            }

            var defaultRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
            var customRaycaster = canvas.gameObject.GetComponent<VrsUIGraphicRaycaster>();
            
            if (customRaycaster)
            {
                Destroy(customRaycaster);
            }

            
            if (defaultRaycaster && !defaultRaycaster.enabled)
            {
                defaultRaycaster.enabled = true;
            }
            if (canvasBoxCollider)
            {
                Destroy(canvasBoxCollider);
            }

            if (canvasRigidBody)
            {
                Destroy(canvasRigidBody);
            }
        }

        private void OnDestroy()
        {
            RemoveCanvas();
        }

        private void OnDisable()
        {
            RemoveCanvas();
        }
    }
}