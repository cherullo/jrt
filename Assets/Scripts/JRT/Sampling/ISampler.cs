using Unity.Mathematics;

namespace JRT.Sampling
{
    public interface ISampler
    {
        int SampleCount { get; }

        MultiSamplingType SamplerType { get; }

        float2[] GetSamplingPoints();

    }
}