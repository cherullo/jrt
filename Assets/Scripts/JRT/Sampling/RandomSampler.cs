using Unity.Mathematics;
using UnityEngine;

namespace JRT.Sampling
{
    public class RandomSampler : BaseSampler
    {
        [SerializeField]
        private int _sampleCount;

        public override int SampleCount => _sampleCount;

        public override MultiSamplingType SamplerType => MultiSamplingType.Random;

        public override float2[] GetSamplingPoints()
        {
            float2[] ret = new float2[_sampleCount];

            for (int i = 0; i < _sampleCount; i++)
                ret[i] = new float2(
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    UnityEngine.Random.Range(0.0f, 1.0f)
                    );

            return ret;
        }
    }
}
