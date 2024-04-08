using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.Sampling
{
    public abstract class BaseSampler : MonoBehaviour, ISampler
    {
        public abstract int SampleCount { get; }

        public abstract MultiSamplingType SamplerType { get; }

        public abstract float2[] GetSamplingPoints();

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
