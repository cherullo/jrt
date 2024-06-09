using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace JRT.Data
{
    public struct LightNode 
    {
        public int Index;
        public LightType Type;
        public float Power;
        public float NormalizedAccumulatedPower;
        public float4x4 LocalToWorld;
        public float3 Color;
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

        public float3 CalculateRadiance(ref World world, float4 point, float3 normal, int sampleIndex, out float3 pointToLightDir)
        {
            float4 samplePosition = _GetSample(ref world.Random, sampleIndex, point, normal, out float3 lightDirection);
            float4 pointToLight = samplePosition - point;
            float distance = length(pointToLight.xyz);
            pointToLightDir = pointToLight.xyz / distance;

            Ray toLight = new Ray(point, pointToLightDir);
            int hitIndex = world.ComputeIntersection(toLight, out HitPoint auxHit);

            if (hitIndex == -1)
                return (Type == LightType.AmbientLight) ? Color * Power : 0;

            // TODO: Check if geometry hit is behind light.
            if (world.Geometries[hitIndex].LightIndex == Index)
            {
                float lightIncidenceDecay = max(0.0f, dot(-pointToLightDir, lightDirection));
                return Color * ((SampleArea * lightIncidenceDecay * Power) / (distance * distance));
            }
            else
                return 0.0f;
        }

        private float4 _GetSample(ref RNG Random, int sampleIndex, float4 point, float3 normal, out float3 lightDirection)
        {
            switch (Type)
            {
                default:
                case LightType.Undefined:
                case LightType.PointLight:
                    float4 position = LocalToWorld.c3;
                    lightDirection = normalize(point.xyz - position.xyz);
                    return position;
                    
                case LightType.AreaLight:
                    lightDirection = normalize(LocalToWorld.c2.xyz); // Forward
                    float4 sampleLocalPos = new(Sampler.GetSample(sampleIndex, ref Random) - 0.5f, 0.0f, 1.0f);
                    return mul(LocalToWorld, sampleLocalPos);

                case LightType.AmbientLight:
                    lightDirection = Random.UnitSphere;

                    //if (lightDirection.z < 0)
                    //    lightDirection.z *= -1.0f;

                    //lightDirection = new Hemisphere(point, normal).ToGlobal(-lightDirection);

                    return point - float4(lightDirection, 0);
            }
        }

        public void ChooseRandomSample(ref RNG random, out int sampleIndex, out float sampleProbability)
        {
            switch (Type)
            {
                default:
                case LightType.Undefined:
                case LightType.PointLight:
                    sampleIndex = 0;
                    sampleProbability = 1.0f;
                    break;

                case LightType.AreaLight:
                    if (Sampler.MultiSamplingType == Sampling.MultiSamplingType.FixedPoints)
                    {
                        sampleIndex = random.Index(Sampler.SampleCount);
                        sampleProbability = 1.0f / Sampler.SampleCount;
                    }
                    else
                    {
                        sampleIndex = 0;

                        // Deveria ser 1 / Area = 1 / (SampleCount * SampleArea)
                        // Mas já estamos multiplicando a radiancia por SampleArea na função CalculateRadiance
                        sampleProbability = 1.0f / (Sampler.SampleCount);
                    }
                    break;
                case LightType.AmbientLight:
                    sampleIndex = 0;
                    sampleProbability = 1.0f / (4.0f * PI);
                    //sampleProbability = 1.0f / (2.0f * PI);
                    break;
            }
        }

        public static LightNode Invalid => new LightNode();
    }
}
