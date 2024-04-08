using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct LightNode 
    {
        public int Index;
        public LightType Type;

        public float Power;
        public float3 Color;
        public float4 Position;

        public float SampleArea;

        // Area Lights
        public float3 Direction; 
        public UnsafeList<float3> SamplingPoints;
        
        public bool IsValid()
        {
            return Type != LightType.Undefined;
        }

        public int GetSampleCount()
        {
            switch (Type)
            {
                default:
                case LightType.Undefined:
                case LightType.PointLight:
                    return 1;

                case LightType.AreaLight:
                    return SamplingPoints.Length;
            }
        }

        public float3 CalculateRadiance(World world, float4 point, int sampleIndex, out float4 pointToLightDir)
        {
            float4 samplePosition = _GetSample(sampleIndex, point, out float3 lightDirection);
            float4 pointToLight = samplePosition - point;
            float distance = math.length(pointToLight.xyz);
            pointToLightDir = new float4(pointToLight.xyz / distance, 0.0f);

            Ray toLight = new Ray(point, pointToLight);
            int hitIndex = world.ComputeIntersection(toLight, out HitPoint auxHit);

            // TODO: Check if geometry hit is behind light.

            float lightIncidenceDecay = math.max(0.0f, math.dot(-pointToLightDir.xyz, lightDirection));

            if ((hitIndex == -1) || (world.Geometries[hitIndex].LightIndex == Index))
                return Color * ((SampleArea * lightIncidenceDecay * Power) / (distance * distance));
            else
                return 0.0f;
        }

        private float4 _GetSample(int sampleIndex, float4 point, out float3 lightDirection)
        {
            switch (Type)
            {
                default:
                case LightType.Undefined:
                case LightType.PointLight:
                    lightDirection = math.normalize(point.xyz - Position.xyz);
                    return Position;
                    
                case LightType.AreaLight:
                    lightDirection = Direction;
                    return new float4(SamplingPoints[sampleIndex], 1.0f);
            }
        }

        public static LightNode Invalid => new LightNode();
    }
}
