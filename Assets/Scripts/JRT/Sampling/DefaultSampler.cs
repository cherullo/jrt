using Unity.Mathematics;

namespace JRT.Sampling
{
    public class DefaultSampler : ISampler
    {
        private DefaultSampler() { }

        public int SampleCount => 1;

        public MultiSamplingType SamplerType => MultiSamplingType.FixedPoints;

        public float2[] GetSamplingPoints()
        {
            return new float2[1] { 0.5f };
        }

        private static DefaultSampler _instance = new DefaultSampler();
        public static DefaultSampler Instance { get => _instance; }
    }
}
