using JD.Extensions;
using UnityEngine;

namespace JD.UI.Utility.Extensions
{
    public static class ColorExtensions
    {
        public static Color ToGray(this Color color, float value, float alpha = -1)
        {
            if (Mathf.Approximately(value, 1))
                return color.ToAlpha(alpha >= 0 ? alpha : color.a);
            
            if (color == Color.white)
            {
                var c2 = color * value;
                c2.a = alpha >= 0 ? alpha : color.a;
                return c2;
            }
            
            var hsl = ColorUtils.RGB2HSL(color);
            hsl.y *= value;
            hsl.z *= value;
            
            var c = ColorUtils.HSL2RGB(hsl);
            c.a = alpha >= 0 ? alpha : color.a;
            
            return c;
        }
    }
}