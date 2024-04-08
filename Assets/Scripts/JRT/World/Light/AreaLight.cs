using JRT.Data;
using JRT.Sampling;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.World.Light
{
    public class AreaLight : BaseLightNode
    {
        private UnsafeList<float3> _samplingPoints;

        public ISampler Sampler
        {
            get
            {
                // I'm doing this because Awake doesn't usually run on editor.
                return GetComponent<ISampler>() ?? DefaultSampler.Instance;
            }
        }

        public override Data.LightType GetLightType()
        {
            return Data.LightType.AreaLight;
        }

        public unsafe override LightNode GetNodeData()
        {
            LightNode node = GetBaseData();

            if (_samplingPoints.IsCreated == false)
            {
                var temp = GenerateSamplingPoints();
                _samplingPoints = new UnsafeList<float3>(temp.Length, AllocatorManager.Persistent);
                for (int i = 0; i < temp.Length; i++) 
                    _samplingPoints.AddNoResize(temp[i]);
            }

            node.SamplingPoints = _samplingPoints;
            node.Direction = transform.forward;

            node.SampleArea = _CalculateLightArea() / _samplingPoints.Length;
            Debug.Log($"SampleArea: {node.SampleArea}");

            return node;
        }

        private float _CalculateLightArea()
        {
            Vector3 corner1 = transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0.0f));
            Vector3 corner2 = transform.TransformPoint(new Vector3( 0.5f, -0.5f, 0.0f));
            Vector3 corner3 = transform.TransformPoint(new Vector3(-0.5f,  0.5f, 0.0f));

            Vector3 v1 = corner2 - corner1;
            Vector3 v2 = corner3 - corner1;

            return Vector3.Cross(v1, v2).magnitude;
        }

        public float3[] GenerateSamplingPoints()
        {
            float2[] samplingPoints = Sampler.GetSamplingPoints();
            float3[] ret = new float3[samplingPoints.Length];

            for (int i = 0; i < samplingPoints.Length; i++)
            {
                float3 localCoords = new(samplingPoints[i] - 0.5f, 0.0f);

                ret[i] = transform.TransformPoint(localCoords);
            }

            return ret;
        }

        private void OnDestroy()
        {
            _samplingPoints.Dispose();
        }
    }
}
