using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace JRT.Data
{
    public static class Utils 
    {
        public static float MIS_BalanceHeuristic(float this_pdf, float other_pdf)
        {
            return this_pdf / (this_pdf + other_pdf);
        }

        public static float GeometricFactor(float4 point, float4 lightPoint, float3 lightDirection)
        {
            float3 lightToPoint = (point - lightPoint).xyz;
            float distance = length(lightToPoint);

            return max(0.0f, dot(lightToPoint, lightDirection)) / (0.001f + distance * distance * distance);
                
        }
    }
}
