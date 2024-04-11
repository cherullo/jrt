using Unity.Mathematics;
using UnityEngine;

namespace JRT.Sampling
{
    public class RegularSampler : BaseSampler
    {
        [SerializeField]
        protected int _rows;

        [SerializeField]
        protected int _columns;
        
        public override int SampleCount => _rows * _columns;

        public override string Name => "Regular Sampler";

        public override MultiSamplingType Type => MultiSamplingType.FixedPoints;

        protected float2[] _samplingPoints;

        public override float2[] GetSamplingPoints()
        {
            if (_samplingPoints == null)
            {
                _samplingPoints = new float2[SampleCount];

                float pitchX = 1.0f / _columns;
                float pitchY = 1.0f / _rows;

                for (int iy = 0; iy < _rows; iy++)
                {
                    float y = (iy + 0.5f) * pitchY;

                    for (int ix = 0; ix < _columns; ix++)
                    {
                        float2 point = new((ix + 0.5f) * pitchX, y);
                        _samplingPoints[ix + iy * _columns] = point;
                    }
                }

                _PrintPattern(_samplingPoints);
            }

            return _samplingPoints;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _rows = Mathf.Max(1, _rows);
            _columns = Mathf.Max(1, _columns);

            _samplingPoints = null;
        }
#endif
    }
}
