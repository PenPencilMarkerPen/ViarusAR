

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Vrs.Internal
{
    public struct PointerEventArgs
    {
        public uint controllerIndex;
        public uint flags;
        public float distance;
        public Transform target;
    }

    public delegate void PointerEventHandler(object sender, PointerEventArgs e);


    public class VrsLaserPointer : MonoBehaviour
    {
        public Color color = Color.white;
        public float thickness = 0.004f;
        public GameObject holder;
        public GameObject pointer;

        private GameObject losdot;

        private GameObject hitObject;

        bool isActive = false;
        public bool addRigidBody = false;
        public event PointerEventHandler PointerIn;
        public event PointerEventHandler PointerOut;
        public VrsInstantNativeApi.ViarusDeviceType deviceType = VrsInstantNativeApi.ViarusDeviceType.RightController;

        Transform previousContact = null;

        float zDistance = 200.0f;

        Transform cacheTransform;

        public Transform GetTransform()
        {
            return cacheTransform;
        }

        public void SetHolderLocalPosition(Vector3 localPosition)
        {
            if (holder == null)
            {
                holder = new GameObject("VrsLaserPointer");
                holder.transform.parent = this.transform;
                holder.transform.localPosition = localPosition;
                holder.transform.localRotation = Quaternion.identity;
            } else
            {
                holder.transform.localPosition = localPosition;
            }
        }

        VrsUIPointer mVrsUIPointer;
        private void Awake()
        {
            mVrsUIPointer = GetComponent<VrsUIPointer>();
            if (mVrsUIPointer == null)
            {
                mVrsUIPointer = gameObject.AddComponent<VrsUIPointer>();
            }
        }


        Ray raycast;
        RaycastHit hit;
        
        void Start()
        {
            raycast = new Ray();
            hit = new RaycastHit();
            cacheTransform = transform;
             
            if(holder == null)
            {
                holder = new GameObject("VrsLaserPointer");
                holder.transform.parent = this.transform;
                holder.transform.localPosition = new Vector3(0, -0.005f, 0.08f);
                holder.transform.localRotation = Quaternion.identity;
            }

            pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pointer.transform.parent = holder.transform;
            pointer.transform.localScale = new Vector3(thickness, thickness, zDistance);
            pointer.transform.localPosition = new Vector3(0f, 0f, 50f);
            pointer.transform.localRotation = Quaternion.identity;
            BoxCollider collider = pointer.GetComponent<BoxCollider>();
            if (addRigidBody)
            {
                if (collider)
                {
                    collider.isTrigger = true;
                }
                Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
                rigidBody.isKinematic = true;
            }
            else
            {
                if (collider)
                {
                    Object.Destroy(collider);
                }
            }
            Material newMaterial = new Material(Shader.Find("Unlit/Color"));
            newMaterial.SetColor("_Color", color);
            pointer.GetComponent<MeshRenderer>().material = newMaterial;     
            losdot = Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/VrsLosDot"));
            
            
            losdot.gameObject.name = "LosDot_" + Time.frameCount +(Time.deltaTime * 1000) + "_" + deviceType.ToString();
            var meshRenderer = losdot.GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                meshRenderer.material.renderQueue = 3500;
            }
            losdot.SetActive(false);
        }

        public virtual void OnPointerIn(PointerEventArgs e)
        {
            if (PointerIn != null)
                PointerIn(this, e);
        }

        public virtual void OnPointerOut(PointerEventArgs e)
        {
            if (PointerOut != null)
                PointerOut(this, e);
        }

        void OnDisable()
        {
            if (losdot != null)
            {
                Destroy(losdot);
                losdot = null;
            }
        }

        
        void Update()
        {
            if (!isActive)
            {
                if(holder != null)
                {
                    holder.SetActive(true);
                    isActive = true;
                }
            }

            if(losdot != null && holder != null)
            {
                losdot.SetActive(holder.activeSelf);
                if(pointer != null) pointer.SetActive(holder.activeSelf);
                if (mVrsUIPointer != null)
                {
                    mVrsUIPointer.enabled = holder.activeSelf;
                }
            }

            if(holder == null || pointer == null || !holder.activeSelf)
            {
                return;
            }

            float dist = zDistance;

            raycast.origin = cacheTransform.position;
            raycast.direction = cacheTransform.forward;
     
            bool bHit = Physics.Raycast(raycast, out hit, zDistance); 
#if UNITY_EDITOR
            Debug.DrawRay(cacheTransform.position, cacheTransform.forward, Color.yellow);
#endif

            CheckCanvasGraphicRayCaster(hit.transform, previousContact);

            if (previousContact && previousContact != hit.transform)
            {
                PointerEventArgs args = new PointerEventArgs();
                args.distance = 0f;
                args.flags = 0;
                args.target = previousContact;
                OnPointerOut(args);
                previousContact = null;
            }

            if (bHit && previousContact != hit.transform)
            {
                PointerEventArgs argsIn = new PointerEventArgs();
                argsIn.distance = hit.distance;
                argsIn.flags = 0;
                argsIn.target = hit.transform;
                OnPointerIn(argsIn);
                previousContact = hit.transform;

                hitObject = hit.collider.gameObject;
            }

            if (!bHit)
            {
                previousContact = null;
                hitObject = null;
                if(losdot != null) losdot.SetActive(false);
            }
            
            if (bHit && hit.distance < zDistance)
            {
                dist = hit.distance;
                if (losdot != null)
                {
                    bool isUICanvasObject = (hit.transform.GetComponent<Canvas>() != null) && (hit.transform.GetComponent<VrsUICanvas>());
                    if (isUICanvasObject)
                    {
                        dist = zDistance;
                        losdot.SetActive(false); 
                        var eventSystem = EventSystem.current;
                        
                        if (eventSystem)
                        {
                            var pointerData = new PointerEventData(eventSystem);
                            var raycastResult = new RaycastResult();
                            raycastResult.worldPosition = transform.position;
                            raycastResult.worldNormal = transform.forward;
                            pointerData.pointerCurrentRaycast = raycastResult;
                            var raycastResults= new List<RaycastResult>();
                            eventSystem.RaycastAll(pointerData,raycastResults);
                            
                            for (var i = raycastResults.Count-1; i >=0; i--)
                            {
                                
                                var canvas = raycastResults[i].gameObject.GetComponent<Canvas>();
                                if (canvas)
                                {
                                    raycastResults.RemoveAt(i);
                                }
                            }
                            if (raycastResults.Count > 0)
                            {
                                dist = hit.distance;
                                losdot.SetActive(true);
                                var scale = 0.005f * dist;
                                losdot.transform.localScale = new Vector3(scale,0.002f,scale);
                                losdot.transform.position = hit.point - new Vector3(0, -holder.transform.localPosition.y , 0.01f);    
                            }
                        }
                    }
                    else
                    {
                        losdot.SetActive(true);
                        var scale = 0.005f * dist;
                        losdot.transform.localScale = new Vector3(scale,0.002f,scale);
                        losdot.transform.position = hit.point - new Vector3(0, -holder.transform.localPosition.y , 0.01f);   
                    }
                }
            }

            if (pointer)
            {
                pointer.transform.localScale = new Vector3(thickness, thickness, dist);
                pointer.transform.localPosition = new Vector3(0f, 0f, dist / 2f);   
            }
        }

        public GameObject GetLosDot()
        {
            return losdot;
        }

        private void CheckCanvasGraphicRayCaster(Transform currentHit, Transform lastHit)
        {
            if (currentHit != null && lastHit != null && currentHit != lastHit)
            {
                if (lastHit.GetComponent<VrsUIGraphicRaycaster>() && lastHit.transform.gameObject.activeInHierarchy && lastHit.GetComponent<VrsUIGraphicRaycaster>().enabled)
                {
                    lastHit.GetComponent<VrsUIGraphicRaycaster>().enabled = false;
                }
            }
            if (currentHit != null && lastHit != null && currentHit == lastHit)
            {
                if (currentHit.GetComponent<VrsUIGraphicRaycaster>() && !currentHit.GetComponent<VrsUIGraphicRaycaster>().enabled)
                {
                    currentHit.GetComponent<VrsUIGraphicRaycaster>().enabled = true;
                }
            }
        }
    }
}