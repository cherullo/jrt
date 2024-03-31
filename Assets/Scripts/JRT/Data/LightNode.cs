using System;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct LightNode 
    {
        public int Index;
        public LightType Type;

        public float Power;
        public float4 Color;
        public float4 Position;

        public bool IsValid()
        {
            return Type != LightType.Undefined;
        }

        public float CalculateRadiance(World world, float4 hitPoint, out float4 pointToLightDir)
        {
            float4 pointToLight = Position - hitPoint;
            float distance = math.length(pointToLight.xyz);

            pointToLightDir = new float4(pointToLight.xyz / distance, 0.0f);
            return Power / (distance * distance);
        }

        public static LightNode Invalid => new LightNode();
    }
}
