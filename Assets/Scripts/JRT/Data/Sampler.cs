using JRT.Sampling;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct Sampler 
    {
        public int SampleCount;
        public MultiSamplingType MultiSamplingType;
        public UnsafeList<float2> SamplingPoints;
        public float2 PixelPitch; // Stratified

        public float2 GetSample(int index, ref RNG Random)
        {
            switch (MultiSamplingType)
            {
                case MultiSamplingType.FixedPoints:
                    return SamplingPoints[index];

                case MultiSamplingType.Stratified:
                    return SamplingPoints[index] + PixelPitch * (Random.UnitSquare - 0.5f);

                default:
                case MultiSamplingType.FullRandom:
                    return Random.UnitSquare;
            }
        }
    }
}