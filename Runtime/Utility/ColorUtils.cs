using JD.Extensions;
using UnityEngine;

namespace JD.UI.Utility
{
    public struct RGB
    {
        // 0 - 255
        public int R;
        public int G;
        public int B;

        public RGB(int rgb)
        {
            R = (rgb & 0xFF0000) >> 16;
            G = (rgb & 0xFF00) >> 8;
            B = rgb & 0xFF;
        }

        public int ToInt()
        {
            return (R << 16) | (G << 8) | B;
        }
    }
    
    public struct HSV
    {
        public int H; // 0 - 360
        public int S; // 0 - 100
        public int V; // 0 - 100
    }

    public struct RandomColors
    {
        public HSV HSV1;
        public HSV HSV2;
        public float Diff;
        public float RandomSaturation;
        public float RandomBrightness;
        public int Color1;
        public int Color2;
    }
    
    public static class ColorUtils
    {
        public const float MaxColor = 75;

        // Recreating the code in the shader exactly to get more accurate results
        // Seems to give different results from the other methods, not sure which is better, I guess the algo is slightly different
        private static float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
        
        private static Vector3 Clamp(Vector3 value, float min, float max)
        {
            return new Vector3(Clamp(value.x, min, max), Clamp(value.y, min, max), Clamp(value.z, min, max));
        }
        
        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

        private static Vector3 FMod(Vector3 value, float mod)
        {
            return new Vector3(value.x % mod, value.y % mod, value.z % mod);
        }

        private static float Step(float a, float b)
        {
            return b >= a ? 1.0f : 0.0f;
        }
        
        private static Vector4 Lerp(Vector4 a, Vector4 b, float t)
        {
            return new Vector4(
                Mathf.Lerp(a.x, b.x, t),
                Mathf.Lerp(a.y, b.y, t),
                Mathf.Lerp(a.z, b.z, t),
                Mathf.Lerp(a.w, b.w, t));
        }
        
        private static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(
                Mathf.Lerp(a.x, b.x, t),
                Mathf.Lerp(a.y, b.y, t),
                Mathf.Lerp(a.z, b.z, t));
        }
        
        private static Vector3 Frac(Vector3 a)
        {
            return new Vector3(a.x - (int) a.x, a.y - (int) a.y, a.z - (int) a.z);
        }
        
        public static Color HSL2RGB(Vector3 hsl)
        {
            var c = new Vector3(hsl.x - (int) hsl.x, Clamp(hsl.y, 0, 1), Clamp(hsl.z, 0, 1));
            var rgb = Clamp(Abs(FMod(new Vector3(c.x * 6.0f, c.x * 6.0f + 4.0f, c.x * 6.0f + 2.0f), 6.0f) - new Vector3(3.0f, 3.0f, 3.0f)) - new Vector3(1.0f, 1.0f, 1.0f), 0.0f, 1.0f);
            var v = c.z.ToV3() + c.y * (rgb - 0.5f.ToV3()) * (1.0f - Mathf.Abs(2.0f * c.z - 1.0f));
            return new Color(v.x, v.y, v.z);
        }

        public static Vector3 RGB2HCV(Color color)
        {
            const float epsilon = 1e-10f;
            
            var bg = new Vector4(color.b, color.g, -1.0f, 2.0f / 3.0f);
            var gb = new Vector4(color.g, color.b, 0.0f, -1.0f / 3.0f);
            var p = Lerp(bg, gb, Step(color.b, color.g));

            var q1 = new Vector4(p.x, p.y, p.w, color.r);
            var q2 = new Vector4(color.r, p.y, p.z, p.x);
            var q = Lerp(q1, q2, Step(p.x, color.r));
            var c = q.x - Mathf.Min(q.w, q.y);
            var h = Mathf.Abs((q.w - q.y) / (6 * c + epsilon) + q.z);
            return new Vector3(h, c, q.x);
        }

        public static Vector3 RGB2HSL(Color color)
        {
            const float epsilon = 1e-10f;
            
            var hcv = RGB2HCV(color);
            var l = hcv.z - hcv.y * 0.5f;
            var s = hcv.y / (1 - Mathf.Abs(l * 2 - 1) + epsilon);
            
            return new Vector3(hcv.x, s, l);
        }

        public static Vector3 Multiply(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
        
        public static Vector3 RGB2HSV_v2(Color color)
        {
            var K = new Vector4(0.0f, -1.0f / 3.0f, 2.0f / 3.0f, -1.0f);
            var p = Lerp(new Vector4( color.b, color.g, K.w, K.z), new Vector4(color.g, color.b, K.x, K.y), Step(color.b, color.g));
            var q = Lerp(new Vector4(p.x, p.y, p.w, color.r), new Vector4(color.r, p.y, p.z, p.x), Step(p.x, color.r));

            var d = q.x - Mathf.Min(q.w, q.y);
            var e = 1.0e-10f;
            return new Vector3(Mathf.Abs(q.z + (q.w - q.y) / (6.0f * d + e)), d / (q.x + e), q.x);
        }

        public static Color HSV2RGB_v2(Vector3 hsv)
        {
            var K = new Vector4(1.0f, 2.0f / 3.0f, 1.0f / 3.0f, 3.0f);
            var p = Abs(Frac(new Vector3(hsv.x, hsv.x, hsv.x) + new Vector3(K.x, K.y, K.z)) * 6.0f - new Vector3(K.w, K.w, K.w));
            var r = Multiply(hsv.z.ToV3(), Lerp(new Vector3(K.x, K.x, K.x), Clamp(p - new Vector3(K.x, K.x, K.x), 0, 1), hsv.y));
            return new Color(r.x, r.y, r.z, 1.0f);
        }
        
        public static int Random(int a, int b)
        {
            return a + Mathf.FloorToInt((float) Utils.Random.NextDouble() * (b - a));
        }

        public static int RandomColor()
        {
            var hsv = new HSV()
            {
                H = Random(0, 359),
                S = Random(50, 100),
                V = Random(60, 95)
            };

            return HSV2RGB(hsv).ToInt();
        }

        public static HSV RGB2HSV(Color color)
        {
            return RGB2HSV((int) (color.r * 255), (int) (color.g * 255), (int) (color.b * 255));
        }

        public static HSV RGB2HSV(uint rgb)
        {
            var color = new RGB((int) rgb);
            return RGB2HSV(color.R, color.G, color.B);
        }

        public static HSV RGB2HSV(int r, int g, int b) 
        {
            var rf = (r / 255f);
            var gf = (g / 255f);
            var bf = (b / 255f);

            var min = Mathf.Min(rf, gf, bf);
            var max = Mathf.Max(rf, gf, bf);
            var diff = max - min;

            var h = 0f; 
            var s = 0f;
            var v = max;
            
            if (Mathf.Approximately(diff, 0))
            {
                h = (float) Utils.Random.NextDouble(); 
                s = 0;
            }
            else
            {
                s = diff / max;

                var diffR = (((max - rf) / 6f) + (diff / 2f)) / diff;
                var diffG = (((max - gf) / 6f) + (diff / 2f)) / diff;
                var diffB = (((max - bf) / 6f) + (diff / 2f)) / diff;

                if      (Mathf.Approximately(rf, max)) h = diffB - diffG;
                else if (Mathf.Approximately(gf, max)) h = (1 / 3f) + diffR - diffB;
                else if (Mathf.Approximately(bf, max)) h = (2 / 3f) + diffG - diffR;

                if (h < 0) h++;
                if (h > 1) h--;
            }

            return new HSV()
            {
                H = (int)(h * 359),
                S = (int)(s * 100),
                V = (int)(v * 100)
            };
        }
        
        public static RGB HSV2RGB(HSV hsv)
        {
            var h = (hsv.H % 360) / 360f;
            var s = hsv.S / 100f;
            var v = hsv.V / 100f;

            if (s < 0)
            {
                s = 0;
            } 
            else if (s > 1)
            {
                s = 1;
            }

            if (v < 0)
            {
                v = 0;
            } 
            else if (v > 1)
            {
                v = 1;
            }

            var i = Mathf.FloorToInt(h * 6);
            var f = h * 6 - i;

            var m = v * (1 - s);
            var n = v * (1 - s * f);
            var k = v * (1 - s * (1 - f));

            var r = 1f;
            var g = 1f;
            var b = 1f;

            switch (i) {
                case 0:
                    r = v;
                    g = k;
                    b = m;
                    break;
                case 1:
                    r = n;
                    g = v;
                    b = m;
                    break;
                case 2:
                    r = m;
                    g = v;
                    b = k;
                    break;
                case 3:
                    r = m;
                    g = n;
                    b = v;
                    break;
                case 4:
                    r = k;
                    g = m;
                    b = v;
                    break;
                case 5:
                case 6:
                    r = v;
                    g = m;
                    b = n;
                    break;
            }

            return new RGB()
            {
                R = Mathf.FloorToInt(r * 255),
                G = Mathf.FloorToInt(g * 255),
                B = Mathf.FloorToInt(b * 255),
            };
        }

        public static RandomColors RandomColors()
        {
            return RandomColor(-1, 0, 0);
        }

        public static RandomColors RandomColor(int hue = -1, int brightnessMin = 0, int brightnessMax = 0)
        {
            var hsv1 = new HSV()
            {
                H = hue >= 0 ? hue : Random(0, 359),
                S = Random(60, 80), // 50, 75
                V = Random(60 + brightnessMin, 80 + brightnessMax)  // 60, 75
            };

            var color1 = HSV2RGB(hsv1);

            var max = 75f;
            if (hsv1.H < MaxColor)
            {
                max = hsv1.H / MaxColor * 50 + 25;
            } 
            else if (hsv1.H > 360 - MaxColor)
            {
                max = ((360 - hsv1.H) / MaxColor) * 50 + 25;
            }

            var diffSave = Random(0, 100);
            diffSave *= Random(0, 1) == 1 ? -1 : 1;

            var diff = (diffSave / 100f) * max;
            var hue2 = hsv1.H + diff;
            if (hue2 < 0) hue2 = 360 + hue2;
            hue2 = hue2 % 360;

            var randomSaturation = Random(-20, 10); // -20, 20
            var randomBrightness = Random(0, 10);   // 0, 15

            var hsv2 = new HSV()
            {
                H = (int) hue2,
                S = hsv1.S + randomSaturation,
                V = hsv1.V - 40 - randomBrightness // - 30
            };

            var color2 = HSV2RGB(hsv2);

            var colorInt1 = (color1.R << 16) + (color1.G << 8) + color1.B;
            var colorInt2 = (color2.R << 16) + (color2.G << 8) + color2.B;

            return new RandomColors()
            {
                HSV1 = hsv1,
                HSV2 = hsv2,
                Diff = diffSave,
                RandomSaturation = randomSaturation,
                RandomBrightness = randomBrightness,
                Color1 = colorInt1,
                Color2 = colorInt2
            };
        }
    }
}