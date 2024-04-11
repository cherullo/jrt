using System.Text;

using JRT.Data;
using JRT.Utils;

using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.Sampling
{
    public abstract class BaseSampler : MonoBehaviour, ISampler
    {
        private UnsafeList<float2> _samplingPoints;

        public abstract int SampleCount { get; }

        public abstract MultiSamplingType Type { get; }

        public abstract float2[] GetSamplingPoints();

        public abstract string Name { get; }

        public Sampler GetSamplerData()
        {
            if ((Type == MultiSamplingType.FixedPoints) && (_samplingPoints.IsCreated == false))
                _samplingPoints = GetSamplingPoints().ToUnsafeList();

            return new Sampler()
            {
                SampleCount = SampleCount,
                MultiSamplingType = Type,
                SamplingPoints = _samplingPoints
            };
        }

        public void OnDestroy()
        {
            if (_samplingPoints.IsCreated == true)
                _samplingPoints.Dispose();
        }

        protected void _PrintPattern(float2[] points)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < points.Length; i++)
            {
                sb.Append(points[i].x);
                sb.Append("\t");
                sb.Append(points[i].y);
                sb.Append("\r\n");
            }

            Debug.Log(sb.ToString());
        }
    }
}
