using Unity.Mathematics;
using static Unity.Mathematics.math;

using UnityEngine;

namespace JRT
{
    public static class ColorExtensions 
    {
        public static float3 ToFloat3(this Color color)
        {
            return float3(color.r, color.g, color.b);
        }

        public static float4 ToFloat4(this Color color)
        {
            return float4(color.r, color.g, color.b, color.a);
        }

        public static float Luminance(this float3 color)
        {
            return dot(color, float3(0.299f, 0.587f, 0.114f));
        }
    }
}
