using System;
using System.Diagnostics;
using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace JRT.Data
{
    [Serializable]
    public struct MicrofacetData 
    {
        public float3 F_zero;
        public bool Metallic;
        public float Roughness;

        public float3 BRDF(float3 albedo, float3 pointToLightDir, float3 normal, float3 pointToViewDir)
        {
            pointToLightDir = normalize(pointToLightDir);
            pointToViewDir = normalize(pointToViewDir);
            if (Metallic)
            {
                albedo = 0.0f;
            }

            float dot_normal_view = max(0, dot(normal, pointToViewDir));
            float dot_normal_light = max(0, dot(normal, pointToLightDir));

            if ((dot_normal_view < 0.05) || (dot_normal_light < 0.05f))
            {
                return albedo / PI;
            }

            float3 halfVector = normalize(pointToViewDir + pointToLightDir);

            float3 F_Schlick = F_zero + (1.0f - F_zero) * pow(1.0f - max(0, dot(pointToViewDir, halfVector)), 5.0f);

            float alpha = Roughness * Roughness;
            float alpha2 = alpha * alpha;
            float denom = pow(max(0, dot(normal, halfVector)), 2.0f) * (alpha2 - 1.0f) + 1.0f;
            denom = PI * denom * denom + 0.0000001f;
            float D_GGX = alpha2 / denom;

            float k = alpha * 0.5f;
            float G_Schlick_view = dot_normal_view / (dot_normal_view * (1.0f - k) + k);
            float G_Schlick_light = dot_normal_light / (dot_normal_light * (1.0f - k) + k);

            float3 ret = (albedo / PI) + (F_Schlick * D_GGX * G_Schlick_view * G_Schlick_light) / (4.0f * dot_normal_view * dot_normal_light);

            return  math.min(1.0f, ret);
        }
    }
}
