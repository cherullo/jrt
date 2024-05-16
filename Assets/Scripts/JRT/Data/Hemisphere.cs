using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace JRT.Data
{
    public struct Hemisphere
    {
        //public float4 Point;
        //public float3 Normal;

        float3x3 M;

        public Hemisphere(float4 point, float3 normal)
        {
            float3 tangent = float3(1, 0, 0);
            if (abs(dot(tangent, normal)) > 0.9f)
                tangent = float3(0, 1, 0);

            float3 binormal = normalize(cross(normal, tangent));
            tangent = cross(binormal, normal);

            M.c0 = tangent;
            M.c1 = binormal;
            M.c2 = normal;
        }

        public float3 ToGlobal(float3 hemisphereDirection)
        {
            return normalize(mul(M, hemisphereDirection));
        }
    }
}
