using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.Sampling
{
    public class HaltonSampler : BaseSampler
    {
        [SerializeField]
        private int _sampleCount;

        public override int SampleCount => _sampleCount;

        public override MultiSamplingType SamplerType => MultiSamplingType.FixedPoints;

        private float2[] _samplingPoints;

        public override float2[] GetSamplingPoints()
        {
            if (_samplingPoints == null)
            {
                _samplingPoints = new float2[_sampleCount];

                IEnumerator<float> h2 = _HaltonSequence(2);
                IEnumerator<float> h3 = _HaltonSequence(3);

                for (int i = 0; i < _sampleCount; i++)
                {
                    h2.MoveNext();
                    h3.MoveNext();
                    _samplingPoints[i] = new float2(h2.Current, h3.Current);
                }

                _PrintPattern(_samplingPoints);
            }

            return _samplingPoints;
        }

        private float _Halton(int b, int index)
        {
            float result = 0.0f;
            float f = 1.0f;
            while (index > 0)
            {
                f /= b;
                result += f * (index % b);
                index /= b;
            }
            return result;
        }

        private IEnumerator<float> _HaltonSequence(int b)
        {
            float n = 0.0f;
            float d = 1.0f;
            while (true)
            {
                float x = d - n;
                if (x == 1)
                {
                    n = 1;
                    d *= b;
                }
                else
                {
                    float y = d / b;
                    while (x <= y)
                        y /= b;
                    n = (b + 1) * y - x;
                }
                yield return n / d;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _sampleCount = Mathf.Max(1, _sampleCount);

            _samplingPoints = null;
        }
#endif

    }
}
