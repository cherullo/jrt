using JRT.Data;
using Unity.Mathematics;

namespace JRT.Sampling
{
    public interface ISampler
    {
        Sampler GetSamplerData();

        string Name { get; }

        MultiSamplingType Type { get; }

        float2[] GetSamplingPoints();
    }
}