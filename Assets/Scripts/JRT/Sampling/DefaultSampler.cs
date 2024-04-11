using Unity.Mathematics;

namespace JRT.Sampling
{
    public class DefaultSampler : BaseSampler
    {
        public DefaultSampler() { }

        public override string Name => "Center Sample";

        public override int SampleCount => 1;

        public override MultiSamplingType Type => MultiSamplingType.FixedPoints;

        public override float2[] GetSamplingPoints()
        {
            return new float2[1] { 0.5f };
        }

        private static DefaultSampler _instance = new DefaultSampler();
        public static DefaultSampler Instance { get => _instance; }

    }
}
