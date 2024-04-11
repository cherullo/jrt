using JRT.Data;
using Unity.Mathematics;

namespace JRT.Sampling
{
    public class StratifiedRandom : RegularSampler
    {
        public override string Name => "Stratified Sampler";

        public override MultiSamplingType Type => MultiSamplingType.Stratified;

        public override Sampler GetSamplerData()
        {
            Sampler ret = base.GetSamplerData();

            ret.PixelPitch = 1.0f / new float2(_columns, _rows);

            return ret;
        }

        //public override float2[] GetSamplingPoints()
        //{
        //    float2 PixelPitch = 1.0f / new float2(_columns, _rows);
        //    float2[] ret = base.GetSamplingPoints();

        //    for (int i = 0; i < ret.Length; i++)
        //        ret[i] += PixelPitch * (new float2(
        //            UnityEngine.Random.Range(0.0f, 1.0f),
        //            UnityEngine.Random.Range(0.0f, 1.0f)
        //            ) - 0.5f);

        //    _PrintPattern(_samplingPoints);

        //    _samplingPoints = null;

        //    return ret;
        //}
    }
}
