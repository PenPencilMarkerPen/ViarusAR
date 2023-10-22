













using System;
using UnityEngine;



namespace Vrs.Internal
{
    [System.Serializable]
    public class VrsProfile
    {
        public VrsProfile Clone()
        {
            return new VrsProfile
            {
                screen = this.screen,
                viewer = this.viewer
            };
        }

        
        
        [System.Serializable]
        public struct Screen
        {
            public float width;   
            public float height;  
            public float border;  
        }

        
        [System.Serializable]
        public struct Lenses
        {
            public float separation;     
            public float offset;         
            public float screenDistance; 

            public int alignment;  
                                   
                                   

            public const int AlignTop = -1;    
            public const int AlignCenter = 0;  
            public const int AlignBottom = 1;  
        }

        
        
        
        
        [System.Serializable]
        public struct MaxFOV
        {
            public float outer;  
            public float inner;  
            public float upper;  
            public float lower;  
        }

        
        
        [System.Serializable]
        public struct Distortion
        {
            private float[] coef;
            public float[] Coef
            {
                get
                {
                    return coef;
                }
                set
                {
                    if (value != null)
                    {
                        coef = (float[])value.Clone();
                    }
                    else
                    {
                        coef = null;
                    }
                }
            }

            public float distort(float r)
            {
                float r2 = r * r;
                float ret = 0;
                for (int j = coef.Length - 1; j >= 0; j--)
                {
                    ret = r2 * (ret + coef[j]);
                }
                return (ret + 1) * r;
            }

            public float distortInv(float radius)
            {
                
                float r0 = 0;
                float r1 = 1;
                float dr0 = radius - distort(r0);
                while (Mathf.Abs(r1 - r0) > 0.0001f)
                {
                    float dr1 = radius - distort(r1);
                    float r2 = r1 - dr1 * ((r1 - r0) / (dr1 - dr0));
                    r0 = r1;
                    r1 = r2;
                    dr0 = dr1;
                }
                return r1;
            }
        }

        
        
        [System.Serializable]
        public struct Viewer
        {
            public Lenses lenses;
            public MaxFOV maxFOV;
            public Distortion distortion;
            public Distortion inverse;
        }

        
        public Screen screen;

        
        public Viewer viewer;

        
        public float VerticalLensOffset
        {
            get
            {
                return (viewer.lenses.offset - screen.border - screen.height / 2) * viewer.lenses.alignment;
            }
        }

        
        public enum ScreenSizes
        {
            Nexus5,
            Nexus6,
            GalaxyS6,
            GalaxyNote4,
            LGG3
        };

        
        public static readonly Screen Nexus5 = new Screen
        {
            width = 0.110f,
            height = 0.062f,
            border = 0.004f
        };

        
        public static readonly Screen Nexus6 = new Screen
        {
            width = 0.133f,
            height = 0.074f,
            border = 0.004f
        };

        
        public static readonly Screen GalaxyS6 = new Screen
        {
            width = 0.114f,
            height = 0.0635f,
            border = 0.0035f
        };

        
        public static readonly Screen GalaxyNote4 = new Screen
        {
            width = 0.125f,
            height = 0.0705f,
            border = 0.0045f
        };

        
        public static readonly Screen LGG3 = new Screen
        {
            width = 0.121f,
            height = 0.068f,
            border = 0.003f
        };

        
        public enum ViewerTypes
        {
            CardboardJun2014,
            CardboardMay2015,
            GoggleTechC1Glass,
        };

        
        public static readonly Viewer CardboardJun2014 = new Viewer
        {
            lenses = {
      separation = 0.060f,
      offset = 0.035f,
      screenDistance = 0.042f,
      alignment = Lenses.AlignBottom,
    },
            maxFOV = {
      outer = 40.0f,
      inner = 40.0f,
      upper = 40.0f,
      lower = 40.0f
    },
            distortion = {
      Coef = new [] { 0.441f, 0.156f },
    },
            inverse = ApproximateInverse(new[] { 0.441f, 0.156f })
        };

        
        public static readonly Viewer CardboardMay2015 = new Viewer
        {
            lenses = {
      separation = 0.064f,
      offset = 0.035f,
      screenDistance = 0.039f,
      alignment = Lenses.AlignBottom,
    },
            maxFOV = {
      outer = 60.0f,
      inner = 60.0f,
      upper = 60.0f,
      lower = 60.0f
    },
            distortion = {
      Coef = new [] { 0.34f, 0.55f },
    },
            inverse = ApproximateInverse(new[] { 0.34f, 0.55f })
        };

        
        public static readonly Viewer GoggleTechC1Glass = new Viewer
        {
            lenses = {
      separation = 0.065f,
      offset = 0.036f,
      screenDistance = 0.058f,
      alignment = Lenses.AlignBottom,
    },
            maxFOV = {
      outer = 50.0f,
      inner = 50.0f,
      upper = 50.0f,
      lower = 50.0f
    },
            distortion = {
      Coef = new [] { 0.3f, 0 },
    },
            inverse = ApproximateInverse(new[] { 0.3f, 0 })
        };

        
        public static readonly VrsProfile Default = new VrsProfile
        {
            screen = Nexus5,
            viewer = CardboardJun2014
        };

        
        public static VrsProfile GetKnownProfile(ScreenSizes screenSize, ViewerTypes deviceType)
        {
            Screen screen;
            switch (screenSize)
            {
                case ScreenSizes.Nexus6:
                    screen = Nexus6;
                    break;
                case ScreenSizes.GalaxyS6:
                    screen = GalaxyS6;
                    break;
                case ScreenSizes.GalaxyNote4:
                    screen = GalaxyNote4;
                    break;
                case ScreenSizes.LGG3:
                    screen = LGG3;
                    break;
                default:
                    screen = Nexus5;
                    break;
            }
            Viewer device;
            switch (deviceType)
            {
                case ViewerTypes.CardboardMay2015:
                    device = CardboardMay2015;
                    break;
                case ViewerTypes.GoggleTechC1Glass:
                    device = GoggleTechC1Glass;
                    break;
                default:
                    device = CardboardJun2014;
                    break;
            }
            return new VrsProfile { screen = screen, viewer = device };
        }

        
        
        public void GetLeftEyeVisibleTanAngles(float[] result)
        {
            
            float fovLeft = Mathf.Tan(-viewer.maxFOV.outer * Mathf.Deg2Rad);
            float fovTop = Mathf.Tan(viewer.maxFOV.upper * Mathf.Deg2Rad);
            float fovRight = Mathf.Tan(viewer.maxFOV.inner * Mathf.Deg2Rad);
            float fovBottom = Mathf.Tan(-viewer.maxFOV.lower * Mathf.Deg2Rad);
            
            float halfWidth = screen.width / 4;
            float halfHeight = screen.height / 2;
            
            float centerX = viewer.lenses.separation / 2 - halfWidth;
            float centerY = -VerticalLensOffset;
            float centerZ = viewer.lenses.screenDistance;
            
            float screenLeft = viewer.distortion.distort((centerX - halfWidth) / centerZ);
            float screenTop = viewer.distortion.distort((centerY + halfHeight) / centerZ);
            float screenRight = viewer.distortion.distort((centerX + halfWidth) / centerZ);
            float screenBottom = viewer.distortion.distort((centerY - halfHeight) / centerZ);
            
            result[0] = Math.Max(fovLeft, screenLeft);
            result[1] = Math.Min(fovTop, screenTop);
            result[2] = Math.Min(fovRight, screenRight);
            result[3] = Math.Max(fovBottom, screenBottom);
        }

        
        
        public void GetLeftEyeNoLensTanAngles(float[] result)
        {
            
            float fovLeft = viewer.distortion.distortInv(Mathf.Tan(-viewer.maxFOV.outer * Mathf.Deg2Rad));
            float fovTop = viewer.distortion.distortInv(Mathf.Tan(viewer.maxFOV.upper * Mathf.Deg2Rad));
            float fovRight = viewer.distortion.distortInv(Mathf.Tan(viewer.maxFOV.inner * Mathf.Deg2Rad));
            float fovBottom = viewer.distortion.distortInv(Mathf.Tan(-viewer.maxFOV.lower * Mathf.Deg2Rad));
            
            float halfWidth = screen.width / 4;
            float halfHeight = screen.height / 2;
            
            float centerX = viewer.lenses.separation / 2 - halfWidth;
            float centerY = -VerticalLensOffset;
            float centerZ = viewer.lenses.screenDistance;
            
            float screenLeft = (centerX - halfWidth) / centerZ;
            float screenTop = (centerY + halfHeight) / centerZ;
            float screenRight = (centerX + halfWidth) / centerZ;
            float screenBottom = (centerY - halfHeight) / centerZ;
            
            result[0] = Math.Max(fovLeft, screenLeft);
            result[1] = Math.Min(fovTop, screenTop);
            result[2] = Math.Min(fovRight, screenRight);
            result[3] = Math.Max(fovBottom, screenBottom);
        }

        
        
        public Rect GetLeftEyeVisibleScreenRect(float[] undistortedFrustum)
        {
            float dist = viewer.lenses.screenDistance;
            float eyeX = (screen.width - viewer.lenses.separation) / 2;
            float eyeY = VerticalLensOffset + screen.height / 2;
            float left = (undistortedFrustum[0] * dist + eyeX) / screen.width;
            float top = (undistortedFrustum[1] * dist + eyeY) / screen.height;
            float right = (undistortedFrustum[2] * dist + eyeX) / screen.width;
            float bottom = (undistortedFrustum[3] * dist + eyeY) / screen.height;
            return new Rect(left, bottom, right - left, top - bottom);
        }

        public static float GetMaxRadius(float[] tanAngleRect)
        {
            float x = Mathf.Max(Mathf.Abs(tanAngleRect[0]), Mathf.Abs(tanAngleRect[2]));
            float y = Mathf.Max(Mathf.Abs(tanAngleRect[1]), Mathf.Abs(tanAngleRect[3]));
            return Mathf.Sqrt(x * x + y * y);
        }

        
        
        
        
        
        
        
        
        
        
        
        private static double[] solveLinear(double[,] a, double[] y)
        {
            int n = a.GetLength(0);

            
            
            
            
            
            
            for (int j = 0; j < n - 1; ++j)
            {
                for (int k = j + 1; k < n; ++k)
                {
                    double p = a[k, j] / a[j, j];
                    for (int i = j + 1; i < n; ++i)
                    {
                        a[k, i] -= p * a[j, i];
                    }
                    y[k] -= p * y[j];
                }
            }
            
            

            double[] x = new double[n];

            
            for (int j = n - 1; j >= 0; --j)
            {
                double v = y[j];
                for (int i = j + 1; i < n; ++i)
                {
                    v -= a[j, i] * x[i];
                }
                x[j] = v / a[j, j];
            }

            return x;
        }

        
        
        
        
        
        
        
        
        
        
        private static double[] solveLeastSquares(double[,] matA, double[] vecY)
        {
            int numSamples = matA.GetLength(0);
            int numCoefficients = matA.GetLength(1);
            if (numSamples != vecY.Length)
            {
                Debug.LogError("Matrix / vector dimension mismatch");
                return null;
            }

            
            double[,] matATA = new double[numCoefficients, numCoefficients];
            for (int k = 0; k < numCoefficients; ++k)
            {
                for (int j = 0; j < numCoefficients; ++j)
                {
                    double sum = 0.0;
                    for (int i = 0; i < numSamples; ++i)
                    {
                        sum += matA[i, j] * matA[i, k];
                    }
                    matATA[j, k] = sum;
                }
            }

            
            double[] vecATY = new double[numCoefficients];
            for (int j = 0; j < numCoefficients; ++j)
            {
                double sum = 0.0;
                for (int i = 0; i < numSamples; ++i)
                {
                    sum += matA[i, j] * vecY[i];
                }
                vecATY[j] = sum;
            }

            
            return solveLinear(matATA, vecATY);
        }

        
        public static Distortion ApproximateInverse(float[] coef, float maxRadius = 1,
                                                    int numSamples = 100)
        {
            return ApproximateInverse(new Distortion { Coef = coef }, maxRadius, numSamples);
        }

        
        public static Distortion ApproximateInverse(Distortion distort, float maxRadius = 1,
                                                    int numSamples = 100)
        {
            const int numCoefficients = 6;

            
            
            
            
            
            
            
            
            
            
            double[,] matA = new double[numSamples, numCoefficients];
            double[] vecY = new double[numSamples];
            for (int i = 0; i < numSamples; ++i)
            {
                float r = maxRadius * (i + 1) / (float)numSamples;
                double rp = distort.distort(r);
                double v = rp;
                for (int j = 0; j < numCoefficients; ++j)
                {
                    v *= rp * rp;
                    matA[i, j] = v;
                }
                vecY[i] = r - rp;
            }
            double[] vecK = solveLeastSquares(matA, vecY);
            
            float[] coefficients = new float[vecK.Length];
            for (int i = 0; i < vecK.Length; ++i)
            {
                coefficients[i] = (float)vecK[i];
            }
            return new Distortion { Coef = coefficients };
        }
    }
    
}