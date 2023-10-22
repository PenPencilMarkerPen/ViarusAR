













using UnityEngine;


namespace Vrs.Internal
{
    public class VrsCameraUtils
    {
        public static void FixProjection(Rect camRect, float nearClipPlane, float farClipPlane,
                                         ref Matrix4x4 proj)
        {
            
            
            proj[0, 0] *= camRect.height / camRect.width / 2;

            
            
            proj[2, 2] = (nearClipPlane + farClipPlane) / (nearClipPlane - farClipPlane);
            proj[2, 3] = 2 * nearClipPlane * farClipPlane / (nearClipPlane - farClipPlane);
        }

        public static Rect FixViewport(Rect rect, Rect viewport, bool isRightEye)
        {
            
            
            if (isRightEye)
            {
                rect.x -= 0.5f;
            }
            rect.width *= 2 * viewport.width;
            rect.x = viewport.x + 2 * rect.x * viewport.width;
            rect.height *= viewport.height;
            rect.y = viewport.y + rect.y * viewport.height;
            return rect;
        }

        public static Rect FixEditorViewport(Rect rect, float profileAspect, float windowAspect)
        {
            float aspectComparison = profileAspect / windowAspect;
            if (aspectComparison < 1)
            {
                rect.width *= aspectComparison;
                rect.x *= aspectComparison;
                rect.x += (1 - aspectComparison) / 2;
            }
            else
            {
                rect.height /= aspectComparison;
                rect.y /= aspectComparison;
            }
            return rect;
        }

        public static void ZoomStereoCameras(float matchByZoom, float matchMonoFOV, float monoProj11,
          ref Matrix4x4 proj)
        {
            float lerp = Mathf.Clamp01(matchByZoom) * Mathf.Clamp01(matchMonoFOV);
            
            float zoom = 1 / Mathf.Lerp(1 / proj[1, 1], 1 / monoProj11, lerp) / proj[1, 1];
            proj[0, 0] *= zoom;
            proj[1, 1] *= zoom;
        }

        
        
        public static void RotationMatrix_to_Quaternion(ref float[] quaternion, float[] r)
        {
            quaternion = new float[4] { 0, 0, 0, 0 };


            
            float fourWSquaredMinusl = r[0] + r[4] + r[8];
            float fourXSquaredMinusl = r[0] - r[4] - r[8];
            float fourYSquaredMinusl = r[4] - r[0] - r[8];
            float fourZSquaredMinusl = r[8] - r[0] - r[4];

            int biggestIndex = 0;
            float fourBiggestSqureMinus1 = fourWSquaredMinusl;
            if (fourXSquaredMinusl > fourBiggestSqureMinus1)
            {
                fourBiggestSqureMinus1 = fourXSquaredMinusl;
                biggestIndex = 1;
            }
            if (fourYSquaredMinusl > fourBiggestSqureMinus1)
            {
                fourBiggestSqureMinus1 = fourYSquaredMinusl;
                biggestIndex = 2;
            }
            if (fourZSquaredMinusl > fourBiggestSqureMinus1)
            {
                fourBiggestSqureMinus1 = fourZSquaredMinusl;
                biggestIndex = 3;
            }

            
            float biggestVal = Mathf.Sqrt(fourBiggestSqureMinus1 + 1.0f) * 0.5f;
            float mult = 0.25f / biggestVal;

            
            switch (biggestIndex)
            {
                case 0:
                    quaternion[3] = biggestVal;
                    quaternion[0] = (r[5] - r[7]) * mult;
                    quaternion[1] = (r[6] - r[2]) * mult;
                    quaternion[2] = (r[1] - r[3]) * mult;
                    break;
                case 1:
                    quaternion[0] = biggestVal;
                    quaternion[3] = (r[5] - r[7]) * mult;
                    quaternion[1] = (r[1] + r[3]) * mult;
                    quaternion[2] = (r[6] + r[2]) * mult;
                    break;
                case 2:
                    quaternion[1] = biggestVal;
                    quaternion[3] = (r[6] - r[2]) * mult;
                    quaternion[0] = (r[1] + r[3]) * mult;
                    quaternion[2] = (r[5] + r[7]) * mult;
                    break;
                case 3:
                    quaternion[2] = biggestVal;
                    quaternion[3] = (r[1] - r[3]) * mult;
                    quaternion[0] = (r[6] + r[2]) * mult;
                    quaternion[1] = (r[5] + r[7]) * mult;
                    break;

            }
        }

        
        
        
        
        
        
        public static void RotationMatrixToEulerAngles(ref float[] eulerAngle, float[] rm)
        {
            float sy = Mathf.Sqrt(rm[0] * rm[0] + rm[3] * rm[3]);

            bool singular = sy < 1e-6; 

            float x, y, z;
            if (!singular)
            {
                x = Mathf.Atan2(rm[7], rm[8]);
                y = Mathf.Atan2(-rm[6], sy);
                z = Mathf.Atan2(rm[3], rm[0]);
            }
            else
            {
                x = Mathf.Atan2(-rm[5], rm[4]);
                y = Mathf.Atan2(-rm[6], sy);
                z = 0;
            }
            x = x * 180.0f / Mathf.PI;
            y = y * 180.0f / Mathf.PI;
            z = z * 180.0f / Mathf.PI;
            eulerAngle = new float[3] { x, y, z };

        }
    }
}