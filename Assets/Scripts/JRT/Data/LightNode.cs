using Unity.Mathematics;

namespace JRT.Data
{
    public struct LightNode 
    {
        public int Index;

        public LightType Type;
        public float Power;
        public float3 Color;
        public float4x4 LocalToWorld;
        public float SampleArea;

        // Area Lights
        public Sampler Sampler;
        
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
                    return Sampler.SampleCount;
            }
        }

        public float3 CalculateRadiance(ref World world, float4 point, int sampleIndex, out float4 pointToLightDir)
        {
            float4 samplePosition = _GetSample(ref world.Random, sampleIndex, point, out float3 lightDirection);
            float4 pointToLight = samplePosition - point;
            float distance = math.length(pointToLight.xyz);
            pointToLightDir = new float4(pointToLight.xyz / distance, 0.0f);

            Ray toLight = new Ray(point, pointToLight);
            int hitIndex = world.ComputeIntersection(toLight, out HitPoint auxHit);

            float lightIncidenceDecay = math.max(0.0f, math.dot(-pointToLightDir.xyz, lightDirection));

            // TODO: Check if geometry hit is behind light.
            if ((hitIndex == -1) || (world.Geometries[hitIndex].LightIndex == Index))
                return Color * ((SampleArea * lightIncidenceDecay * Power) / (distance * distance));
            else
                return 0.0f;
        }

        private float4 _GetSample(ref RNG Random, int sampleIndex, float4 point, out float3 lightDirection)
        {
            switch (Type)
            {
                default:
                case LightType.Undefined:
                case LightType.PointLight:
                    float4 position = LocalToWorld.c3;
                    lightDirection = math.normalize(point.xyz - position.xyz);
                    return position;
                    
                case LightType.AreaLight:
                    lightDirection = math.normalize(LocalToWorld.c2.xyz); // Forward
                    float4 sampleLocalPos = new(Sampler.GetSample(sampleIndex, ref Random) - 0.5f, 0.0f, 1.0f);
                    return math.mul(LocalToWorld, sampleLocalPos);
            }
        }

        public static LightNode Invalid => new LightNode();
    }
}
