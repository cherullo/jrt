using Unity.Mathematics;
using UnityEngine;

namespace JRT
{
    public static class ColorExtensions 
    {
        public static float3 ToFloat3(this Color color)
        {
            return new float3(color.r, color.g, color.b);
        }

        public static float4 ToFloat4(this Color color)
        {
            return new float4(color.r, color.g, color.b, color.a);
        }
    }
}
