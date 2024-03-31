using System;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct Material
    {
        public float3 DiffuseColor;
        public float3 SpecularColor;
        public float Shininess;

        public float3 CalculateColor(World world, Ray ray, HitPoint hitPoint)
        {
            float3 color = float3.zero;
            float4 pointToEyeDir = new float4(math.normalize((ray.Start - hitPoint.Point).xyz), 0.0f);

            for (int lightIndex = 0; lightIndex < world.Lights.Length; lightIndex++)
            {
                LightNode light = world.Lights[lightIndex];
                float L = light.CalculateRadiance(world, hitPoint.Point, out float4 pointToLightDir);

                float3 reflect = math.reflect(-pointToLightDir.xyz, hitPoint.Normal.xyz);

                color += L * (DiffuseColor * math.dot(hitPoint.Normal, pointToLightDir)
                              + SpecularColor * math.pow(math.max(0.0f, math.dot(reflect, pointToEyeDir.xyz)), Shininess));
            }

            return color;
        }
    }
}
