using System.Runtime.InteropServices;
using UnityEngine;
namespace Vrs.Internal
{
    public class FpsStatistics : MonoBehaviour
    {
        ViarusService mViarusService;
        TextMesh textMesh;
        
        void Start()
        {
            textMesh = GetComponent<TextMesh>();
            if (VrsViewer.Instance.ShowFPS)
            {
                mViarusService = VrsViewer.Instance.GetViarusService();
                if(mViarusService != null)
                {
                    mViarusService.SetEnableFPS(true);
                }
            } else
            {
                Debug.Log("Display FPS is disabled.");
            }

            Debug.Log("TrackerPosition=" + VrsViewer.Instance.TrackerPosition);
        }

        
        void Update()
        {
            if(mViarusService != null)
            {
                float[] fps = mViarusService.GetFPS();
                textMesh.text = "FBO " + fps[0] + ", DTR " + fps[1];
            }
        }
    }
}