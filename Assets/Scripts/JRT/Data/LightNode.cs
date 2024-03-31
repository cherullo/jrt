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

        public float CalculateRadiance(World world, float4 point, out float4 pointToLightDir)
        {
            float4 pointToLight = Position - point;
            float distance = math.length(pointToLight.xyz);
            pointToLightDir = new float4(pointToLight.xyz / distance, 0.0f);

            Ray toLight = new Ray(point, pointToLight);
            int hitIndex = world.ComputeIntersection(toLight, out HitPoint auxHit);

            if ((hitIndex == -1) || (world.Geometries[hitIndex].LightIndex == Index))
                return Power / (distance * distance);
            else
                return 0.0f;
        }

        public static LightNode Invalid => new LightNode();
    }
}
