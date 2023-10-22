













using UnityEngine;
using System.Collections.Generic;











namespace Vrs.Internal
{
    [RequireComponent(typeof(Camera))]
    [AddComponentMenu("VRS/Internal/VrsPostRender")]
    public class VrsPostRender : MonoBehaviour
    {

        
        public Camera cam { get; private set; }

        

        
        private const int kMeshWidth = 20;
        private const int kMeshHeight = 20;
        
        private const bool kDistortVertices = true;

        private Mesh distortionMesh;
        private Material meshMaterial;

        Mesh quadMesh;
        private float centerWidthPx;
        private float buttonWidthPx;
        private float xScale;
        private float yScale;
        private Matrix4x4 xfm;

        void Reset()
        {
#if UNITY_EDITOR
            
            
            var cam = GetComponent<Camera>();
#endif
            cam.clearFlags = CameraClearFlags.Depth;
            cam.backgroundColor = Color.black;  
            cam.orthographic = true;
            cam.orthographicSize = 0.5f;
            cam.cullingMask = 0;
            cam.useOcclusionCulling = false;
            cam.depth = 100;
            if (VrsGlobal.isVR9Platform)
            {
                
                cam.clearFlags = CameraClearFlags.Nothing;
            }
        }

        void Awake()
        {
            cam = GetComponent<Camera>();
            Reset();
            meshMaterial = new Material(Shader.Find("NAR/UnlitTexture"));
        }


        private float aspectComparison;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        void OnPreCull()
        {
            
            float realAspect = (float)Screen.width / Screen.height;
            float fakeAspect = VrsViewer.Instance.Profile.screen.width / VrsViewer.Instance.Profile.screen.height;
            aspectComparison = fakeAspect / realAspect;
            cam.orthographicSize = 0.5f * Mathf.Max(1, aspectComparison);
        }
#endif

        bool firstDraw = true;
        void OnRenderObject()
        {
            if (Camera.current != cam || !VrsViewer.Instance.SplitScreenModeEnabled)
                return;

            if (!Application.isEditor && VrsGlobal.supportDtr) return;
            
            RenderTexture stereoScreen = VrsViewer.Instance.GetStereoScreen(0);

            if (stereoScreen == null)
            {
                return;
            }

            bool useDFT = VrsViewer.USE_DTR && !VrsGlobal.supportDtr;

            if ((!VrsViewer.USE_DTR || useDFT) && VrsGlobal.distortionEnabled)
            {
                if (distortionMesh == null || VrsViewer.Instance.ProfileChanged)
                {
                    RebuildDistortionMesh();
                }

                meshMaterial.mainTexture = stereoScreen;
                meshMaterial.SetPass(0);

                if (!firstDraw)
                {
                    if (VrsGlobal.offaxisDistortionEnabled)
                    {
                        int offsetx1 = VrsGlobal.offaxisOffset[0] > 0 ? VrsGlobal.offaxisOffset[0] : 60;
                        int offsetx2 = VrsGlobal.offaxisOffset[1] > 0 ? VrsGlobal.offaxisOffset[1] : 60;
                        int offsety1 = VrsGlobal.offaxisOffset[2] > 0 ? VrsGlobal.offaxisOffset[2] : 10;
                        int offsety2 = VrsGlobal.offaxisOffset[3] > 0 ? VrsGlobal.offaxisOffset[3] : 170;
                        GL.Viewport(new Rect(offsetx1, (offsety1 + offsety2) / 2, Screen.width - offsetx1 - offsetx2, Screen.height - offsety1 - offsety2));
                    }
                    Graphics.DrawMeshNow(distortionMesh, transform.position, transform.rotation);
                }
                else
                {
                    firstDraw = false;
                }
            }
        }

        public Texture PreviewTexture;

        void OnGUI()
        {
            bool useDFT = VrsViewer.USE_DTR && !VrsGlobal.supportDtr;
            if (VrsGlobal.distortionEnabled || (!useDFT && VrsViewer.USE_DTR)) return;
            
            if (!Event.current.type.Equals(EventType.Repaint))
            {
                return;
            }

            if (PreviewTexture != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width / 2, Screen.height), PreviewTexture);
                GUI.DrawTexture(new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height), PreviewTexture);
            }

            if ((!VrsViewer.USE_DTR || useDFT) && !VrsGlobal.distortionEnabled && !firstDraw)
            {
                RenderTexture stereoScreen = VrsViewer.Instance.GetStereoScreen(0);
                if (stereoScreen == null)
                {
                    return;
                }
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), stereoScreen);
            }
            else
            {
                firstDraw = false;
            }
        }

        private void BuildQuadMesh()
        {
            quadMesh = new Mesh();
            quadMesh.vertices = new Vector3[] { new Vector3(-1, -1, 1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, -1, 1) };
            quadMesh.uv = new Vector2[] { new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 0, 1) };
            quadMesh.triangles = new int[]
            { 0, 1, 2,
          0, 2, 3
            };
            quadMesh.UploadMeshData(true);
        }

        private void RebuildDistortionMesh()
        {
            distortionMesh = new Mesh();
            Vector3[] vertices;
            Vector2[] tex;

            int meshWidth = kMeshWidth;
            int meshHeight = kMeshHeight;

            if (VrsGlobal.offaxisDistortionEnabled)
            {
                cam.orthographicSize = 0.5649f;
                meshMaterial = new Material(Shader.Find("NAR/UnlitTextureOffaxis"));

                if (VrsGlobal.meshSize != null)
                {
                    meshWidth = VrsGlobal.meshSize[0];
                    meshHeight = VrsGlobal.meshSize[1];
                }
                ComputeMeshPointsOffAsix(meshWidth, meshHeight, kDistortVertices, out vertices, out tex);
            }
            else
            {
                ComputeMeshPoints(meshWidth, meshHeight, kDistortVertices, out vertices, out tex);
            }

            int[] indices = ComputeMeshIndices(meshWidth, meshHeight, kDistortVertices);
            Color[] colors = ComputeMeshColors(meshWidth, meshHeight, tex, indices, kDistortVertices);

            distortionMesh.vertices = vertices;
            distortionMesh.uv = tex;
            distortionMesh.colors = colors;
            distortionMesh.triangles = indices;
#if !UNITY_5_5_OR_NEWER
    
    distortionMesh.Optimize();
#endif  

            distortionMesh.UploadMeshData(true);
        }

        private static Dictionary<string, float[]> configDict = new Dictionary<string, float[]>();
        private static void ComputeMeshPointsOffAsix(int width, int height, bool distortVertices,
                                              out Vector3[] vertices, out Vector2[] tex)
        {
            configDict.Clear();
            
            string text = VrsGlobal.offaxisDistortionConfigData;
            if (text == null || text.Length == 0)
            {
                TextAsset taCN = Resources.Load<TextAsset>("ViarusDistortionConfig");
                text = taCN.text;
            }

            string[] linesCN = text.Split('\n');
            
            foreach (string line in linesCN)
            {
                if (line == null || line.Length <= 1)
                {
                    continue;
                }
                string[] keyAndValue = line.Split('=');
                
                string[] values = keyAndValue[1].Split(',');
                float[] uvInfo = new float[values.Length];
                for (int i = 0; i < values.Length; i++)
                {
                    uvInfo[i] = float.Parse(values[i]);
                }
                configDict.Add(keyAndValue[0], uvInfo);
            }
            

            vertices = new Vector3[2 * width * height];
            tex = new Vector2[2 * width * height];
            for (int e = 0, vidx = 0; e < 2; e++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++, vidx++)
                    {
                        float x = -1.0f + 2.0f * i / width;
                        float y = -1.0f + 2.0f * j / height;
                        string key = e + "," + i + "," + j;
                        float[] uvInfo = new float[4];
                        configDict.TryGetValue(key, out uvInfo);
                        float u = uvInfo[2];
                        float v = uvInfo[3];
                        
                        x = x / 2;
                        y = y / 2;

                        
                        if (e == 0)
                        {
                            
                            u = u * 0.5f;
                            x = x - 0.5f;
                        }
                        else
                        {
                            
                            u = u * 0.5f + 0.5f;
                            x = x + 0.5f;
                        }

                        vertices[vidx] = new Vector3(x, y, 1);
                        
                        tex[vidx] = new Vector2(u, v);
                        
                    }
                }

            }
        }

        private static void ComputeMeshPoints(int width, int height, bool distortVertices,
                                              out Vector3[] vertices, out Vector2[] tex)
        {
            float[] lensFrustum = new float[4];
            float[] noLensFrustum = new float[4];
            Rect viewport;
            VrsProfile profile = VrsViewer.Instance.Profile;
            profile.GetLeftEyeVisibleTanAngles(lensFrustum);
            profile.GetLeftEyeNoLensTanAngles(noLensFrustum);
            viewport = profile.GetLeftEyeVisibleScreenRect(noLensFrustum);
            vertices = new Vector3[2 * width * height];
            tex = new Vector2[2 * width * height];
            for (int e = 0, vidx = 0; e < 2; e++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++, vidx++)
                    {
                        float u = (float)i / (width - 1);
                        float v = (float)j / (height - 1);
                        float s, t;  
                        if (distortVertices)
                        {
                            
                            s = u;
                            t = v;
                            float x = Mathf.Lerp(lensFrustum[0], lensFrustum[2], u);
                            float y = Mathf.Lerp(lensFrustum[3], lensFrustum[1], v);
                            float d = Mathf.Sqrt(x * x + y * y);
                            float r = profile.viewer.distortion.distortInv(d);
                            float p = x * r / d;
                            float q = y * r / d;
                            u = (p - noLensFrustum[0]) / (noLensFrustum[2] - noLensFrustum[0]);
                            v = (q - noLensFrustum[3]) / (noLensFrustum[1] - noLensFrustum[3]);
                        }
                        else
                        {
                            
                            
                            float p = Mathf.Lerp(noLensFrustum[0], noLensFrustum[2], u);
                            float q = Mathf.Lerp(noLensFrustum[3], noLensFrustum[1], v);
                            float r = Mathf.Sqrt(p * p + q * q);
                            float d = profile.viewer.distortion.distort(r);
                            float x = p * d / r;
                            float y = q * d / r;
                            s = Mathf.Clamp01((x - lensFrustum[0]) / (lensFrustum[2] - lensFrustum[0]));
                            t = Mathf.Clamp01((y - lensFrustum[3]) / (lensFrustum[1] - lensFrustum[3]));
                        }
                        
                        float aspect = profile.screen.width / profile.screen.height;
                        u = (viewport.x + u * viewport.width - 0.5f) * aspect;
                        v = viewport.y + v * viewport.height - 0.5f;
                        vertices[vidx] = new Vector3(u, v, 1);
                        
                        s = (s + e) / 2;
                        tex[vidx] = new Vector2(s, t);
                    }
                }
                float w = lensFrustum[2] - lensFrustum[0];
                lensFrustum[0] = -(w + lensFrustum[0]);
                lensFrustum[2] = w - lensFrustum[2];
                w = noLensFrustum[2] - noLensFrustum[0];
                noLensFrustum[0] = -(w + noLensFrustum[0]);
                noLensFrustum[2] = w - noLensFrustum[2];
                viewport.x = 1 - (viewport.x + viewport.width);
            }
        }

        private static Color[] ComputeMeshColors(int width, int height, Vector2[] tex, int[] indices,
                                                   bool distortVertices)
        {
            Color[] colors = new Color[2 * width * height];
            for (int e = 0, vidx = 0; e < 2; e++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++, vidx++)
                    {
                        colors[vidx] = Color.white;
                        if (distortVertices)
                        {
                            if (i == 0 || j == 0 || i == (width - 1) || j == (height - 1))
                            {
                                colors[vidx] = Color.black;
                            }
                        }
                        else
                        {
                            Vector2 t = tex[vidx];
                            t.x = Mathf.Abs(t.x * 2 - 1);
                            if (t.x <= 0 || t.y <= 0 || t.x >= 1 || t.y >= 1)
                            {
                                colors[vidx] = Color.black;
                            }
                        }
                    }
                }
            }
            return colors;
        }

        private static int[] ComputeMeshIndices(int width, int height, bool distortVertices)
        {
            int[] indices = new int[2 * (width - 1) * (height - 1) * 6];
            int halfwidth = width / 2;
            int halfheight = height / 2;
            for (int e = 0, vidx = 0, iidx = 0; e < 2; e++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++, vidx++)
                    {
                        if (i == 0 || j == 0)
                            continue;
                        
                        
                        if ((i <= halfwidth) == (j <= halfheight))
                        {
                            
                            indices[iidx++] = vidx;
                            indices[iidx++] = vidx - width;
                            indices[iidx++] = vidx - width - 1;
                            indices[iidx++] = vidx - width - 1;
                            indices[iidx++] = vidx - 1;
                            indices[iidx++] = vidx;
                        }
                        else
                        {
                            
                            indices[iidx++] = vidx - 1;
                            indices[iidx++] = vidx;
                            indices[iidx++] = vidx - width;
                            indices[iidx++] = vidx - width;
                            indices[iidx++] = vidx - width - 1;
                            indices[iidx++] = vidx - 1;
                        }
                    }
                }
            }
            return indices;
        }

        
        
        
        private void OnDestroy()
        {
            Debug.Log("VrsPostRender.OnDestroy");
        }
    }
}