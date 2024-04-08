using Unity.Mathematics;

namespace JRT.Sampling
{
    public class DefaultSampler : ISampler
    {
        public int SampleCount => 1;

        public MultiSamplingType SamplerType => MultiSamplingType.FixedPoints;

        public float2[] GetSamplingPoints()
        {
            return new float2[1] { 0.5f };
        }

    }
}
