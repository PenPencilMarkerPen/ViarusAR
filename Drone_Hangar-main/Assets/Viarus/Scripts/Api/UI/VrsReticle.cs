













using UnityEngine;
using UnityEngine.UI;



namespace Vrs.Internal
{
    [AddComponentMenu("VRS/UI/VrsReticle")]
    [RequireComponent(typeof(Renderer))]
    public class VrsReticle : MonoBehaviour, IVrsGazePointer
    {
        
        private int reticleSegments = 20;

        
        public float reticleGrowthSpeed = 10.0f;

        
        private bool showReticle = true;

        
        private Material materialComp;
        

        
        private float reticleInnerAngle = 0.0f;
        
        private float reticleOuterAngle = 0.25f;
        
        private float reticleDistanceInMeters = 50.0f;

        
        private const float kReticleMinInnerAngle = 0.0f;
        
        protected float kReticleMinOuterAngle = 0.25f;
        
        
        private const float kReticleGrowthAngle = 0.4f;

        
        private const float kReticleDistanceMin = 0.45f;
        
        private const float kReticleDistanceMax = 50.0f;

        
        
        private float reticleInnerDiameter = 0.0f;
        private float reticleOuterDiameter = 0.0f;

        
        
        
        [Range(-32767, 32767)]
        public int reticleSortingOrder = 32767;

        GameObject reticlePointer;
        Transform cacheTransform;
        Color tempColor;
        private void Awake()
        {
            cacheTransform = transform;
            reticlePointer = new GameObject("Pointer");
            reticlePointer.transform.parent = cacheTransform;
            reticlePointer.transform.localPosition = Vector3.zero;
            reticlePointer.transform.localRotation = Quaternion.identity;
        }

        void Start()
        {
            CreateReticleVertices();
            Renderer rendererComponent = GetComponent<Renderer>();
            rendererComponent.sortingOrder = reticleSortingOrder;
            materialComp = rendererComponent.material;
            tempColor = new Color(materialComp.color.r, materialComp.color.g, materialComp.color.b, alphaValue);
        }

        public GameObject GetReticlePointer()
        {
            return reticlePointer;
        }

        void OnEnable()
        {
            GazeInputModule.gazePointer = this;
            Debug.Log("VrsReticle OnEnable");
        }

        void OnDisable()
        {
            Debug.Log("VrsReticle OnDisable");
            if (GazeInputModule.gazePointer == this)
            {
                GazeInputModule.gazePointer = null;
                showReticle = false;
            }
            if (headControl != null)
            {
                Destroy(headControl);
                headControl = null;
            }

        }

        private float alphaValue = -1.0f;

        
        private int updatedToFrame = 0;

        private void Update()
        {
            if(GazeInputModule.gazePointer == null && !showReticle)
            {
                UpdateStatus();
            }
        }

        public void UpdateStatus()
        {
            if (updatedToFrame == Time.frameCount) return;

            updatedToFrame = Time.frameCount;
            if (showReticle)
            {
                UpdateDiameters();
            }

            float valueTmp = showReticle ? 1.0f : 0.0f;
            if (valueTmp != alphaValue)
            {
                alphaValue = valueTmp;
                tempColor.r = materialComp.color.r;
                tempColor.g = materialComp.color.g;
                tempColor.b = materialComp.color.b;
                tempColor.a = alphaValue;
                materialComp.color = tempColor;
            }
        }

        
        public void OnGazeEnabled()
        {

        }

        
        public void OnGazeDisabled()
        {

        }

        
        
        
        
        
        
        public void OnGazeStart(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                                bool isInteractive)
        {
            bool IsHoverMode = VrsViewer.Instance != null && VrsViewer.Instance.HeadControl == HeadControl.Hover;
            SetGazeTarget(intersectionPosition, isInteractive);
            if (headControl != null && isInteractive)
            {
                if(IsHoverMode)
                {
                    ViarusHMDControl mVrsHeadControl = headControl.GetComponent<ViarusHMDControl>();
                    mVrsHeadControl.Show();
                    mVrsHeadControl.HandleDown();
                    ViarusHMDControl.eventGameObject = targetObject;
                }
            }
        }

        
        
        
        
        
        
        public void OnGazeStay(Camera camera, GameObject targetObject, Vector3 intersectionPosition,
                               bool isInteractive)
        {
            SetGazeTarget(intersectionPosition, isInteractive);
        }

        
        
        
        
        
        
        
        public void OnGazeExit(Camera camera, GameObject targetObject)
        {
            reticleDistanceInMeters = kReticleDistanceMax;
            reticleInnerAngle = kReticleMinInnerAngle;
            reticleOuterAngle = kReticleMinOuterAngle;
            bool IsHoverMode = VrsViewer.Instance != null && VrsViewer.Instance.HeadControl == HeadControl.Hover;
            if (headControl != null)
            {
                if(IsHoverMode)
                {
                    headControl.GetComponent<ViarusHMDControl>().HandleUp();
                    
                    
                }
            }
        }

        
        
        public void OnGazeTriggerStart(Camera camera)
        {
            
        }

        
        
        public void OnGazeTriggerEnd(Camera camera)
        {
            
        }

        public void GetPointerRadius(out float innerRadius, out float outerRadius)
        {
            float min_inner_angle_radians = Mathf.Deg2Rad * kReticleMinInnerAngle;
            float max_inner_angle_radians = Mathf.Deg2Rad * (kReticleMinInnerAngle + kReticleGrowthAngle);
            innerRadius = 2.0f * Mathf.Tan(min_inner_angle_radians);
            outerRadius = 2.0f * Mathf.Tan(max_inner_angle_radians);
        }
        private void CreateReticleVertices()
        {
            Mesh mesh = new Mesh();
            gameObject.AddComponent<MeshFilter>();
            GetComponent<MeshFilter>().mesh = mesh;

            int segments_count = reticleSegments;
            int vertex_count = (segments_count + 1) * 2;

            #region Vertices

            Vector3[] vertices = new Vector3[vertex_count];

            const float kTwoPi = Mathf.PI * 2.0f;
            int vi = 0;
            for (int si = 0; si <= segments_count; ++si)
            {
                
                
                float angle = (float)si / (float)(segments_count) * kTwoPi;

                float x = Mathf.Sin(angle);
                float y = Mathf.Cos(angle);

                vertices[vi++] = new Vector3(x, y, 0.0f); 
                vertices[vi++] = new Vector3(x, y, 1.0f); 
            }
            #endregion

            #region Triangles
            int indices_count = (segments_count + 1) * 3 * 2;
            int[] indices = new int[indices_count];

            int vert = 0;
            int idx = 0;
            for (int si = 0; si < segments_count; ++si)
            {
                indices[idx++] = vert + 1;
                indices[idx++] = vert;
                indices[idx++] = vert + 2;

                indices[idx++] = vert + 1;
                indices[idx++] = vert + 2;
                indices[idx++] = vert + 3;

                vert += 2;
            }
            #endregion

            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.RecalculateBounds();
            
        }

        private void UpdateDiameters()
        {
            reticleDistanceInMeters =
              Mathf.Clamp(reticleDistanceInMeters, kReticleDistanceMin, kReticleDistanceMax);

            if (reticleInnerAngle < kReticleMinInnerAngle)
            {
                reticleInnerAngle = kReticleMinInnerAngle;
            }

            if (reticleOuterAngle < kReticleMinOuterAngle)
            {
                reticleOuterAngle = kReticleMinOuterAngle;
            }

            float inner_half_angle_radians = Mathf.Deg2Rad * reticleInnerAngle * 0.5f;
            float outer_half_angle_radians = Mathf.Deg2Rad * reticleOuterAngle * 0.5f;

            float inner_diameter = 2.0f * Mathf.Tan(inner_half_angle_radians);
            float outer_diameter = 2.0f * Mathf.Tan(outer_half_angle_radians);

            reticleInnerDiameter =
                Mathf.Lerp(reticleInnerDiameter, inner_diameter, Time.deltaTime * reticleGrowthSpeed);
            reticleOuterDiameter =
                Mathf.Lerp(reticleOuterDiameter, outer_diameter, Time.deltaTime * reticleGrowthSpeed);


            materialComp.SetFloat("_InnerDiameter", reticleInnerDiameter * reticleDistanceInMeters);
            materialComp.SetFloat("_OuterDiameter", reticleOuterDiameter * reticleDistanceInMeters);
            materialComp.SetFloat("_DistanceInMeters", reticleDistanceInMeters);
        }

        internal void Show()
        {
            if (materialComp != null)
            {
                tempColor.r = materialComp.color.r;
                tempColor.g = materialComp.color.g;
                tempColor.b = materialComp.color.b;
                tempColor.a = 1;
                materialComp.color = tempColor;
            }
            showReticle = true;
            
        }

        public bool IsShowing()
        {
            return showReticle;
        }

        private GameObject headControl;
        private Transform headCtrlCanvasTrans;
        public void HeadShow()
        {
            if (headControl == null)
            {
                headControl = (GameObject)Instantiate(Resources.Load<GameObject>("Reticle/NarHeadControl"),  gameObject.transform);
                headCtrlCanvasTrans = headControl.GetComponent<Canvas>().transform;
            }
            else
            {
                
                headControl.GetComponent<ViarusHMDControl>().Show();
            }
            headControl.transform.localRotation = Quaternion.identity;
            Debug.Log("HeadShow");
        }

        public void HeadDismiss()
        {
            if (headControl != null)
            {
                
                headControl.GetComponent<ViarusHMDControl>().Hide();
                Debug.Log("HeadDismiss");
            }
        }

        public void Dismiss()
        {
            if (materialComp != null)
            {
                tempColor.r = materialComp.color.r;
                tempColor.g = materialComp.color.g;
                tempColor.b = materialComp.color.b;
                tempColor.a = 0;
                materialComp.color = tempColor;
            }
            showReticle = false;
            
        }

        private void SetGazeTarget(Vector3 target, bool interactive)
        {
            Vector3 targetLocalPosition = cacheTransform.InverseTransformPoint(target);
            reticlePointer.transform.localPosition = new Vector3(0, 0, targetLocalPosition.z - 0.02f);

            reticleDistanceInMeters =
                Mathf.Clamp(targetLocalPosition.z, kReticleDistanceMin, kReticleDistanceMax);

            bool IsHoverMode = VrsViewer.Instance != null && VrsViewer.Instance.HeadControl == HeadControl.Hover;
            if (IsHoverMode)
            {
                
                headControl.transform.localPosition = new Vector3(0, 0, reticleDistanceInMeters - 0.02f);
                headCtrlCanvasTrans.rotation = Quaternion.identity;
                headControl.GetComponent<ViarusHMDControl>().HandleGazeStay();
                Dismiss();
            }
            else
            {
                reticleInnerAngle = kReticleMinInnerAngle + (interactive ? kReticleGrowthAngle : 0);
                reticleOuterAngle = kReticleMinOuterAngle + (interactive ? kReticleGrowthAngle : 0);
            }
        }

        public void UpdateColor(Color color)
        {
            if(materialComp  != null)
            {
                float alpha = materialComp.color.a;
                alphaValue = alpha;

                tempColor.r = color.r;
                tempColor.g = color.g;
                tempColor.b = color.b;
                tempColor.a = alphaValue;
                materialComp.color = tempColor;
            }
        }

        public void UpdateSize(float size)
        {
            kReticleMinOuterAngle = size;
        }

        public float GetSize()
        {
            return kReticleMinOuterAngle;
        }
    }
}