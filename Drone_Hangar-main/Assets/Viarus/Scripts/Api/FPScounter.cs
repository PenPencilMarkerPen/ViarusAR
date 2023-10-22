











using UnityEngine;

namespace Vrs.Internal
{
    public class FPScounter : MonoBehaviour
    {

        private string fpsFormat;
        private float updateInterval = 0.2f;
        private float accum = .0f;
        private int frames = 0;
        private float timeLeft;
        public static float fpsDeltaTime;

        TextMesh textMesh;
        
        void Start()
        {
            textMesh = GetComponent<TextMesh>();
        }

        
        void Update()
        {
            calculate_fps();
            fpsDeltaTime += Time.deltaTime;
            if (fpsDeltaTime > 1)
            {
                
                fpsDeltaTime = 0;
                if (textMesh != null)
                {
                    textMesh.text = fpsFormat;
                }
            }

        }

        private void calculate_fps()
        {
            timeLeft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            ++frames;

            if (timeLeft <= 0)
            {
                float fps = accum / frames;
                fpsFormat = System.String.Format("{0:F3}fps", fps);
                
                timeLeft = updateInterval;
                accum = .0f;
                frames = 0;
            }
        }
    }
}