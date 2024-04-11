using JRT.Data;
using JRT.Sampling;

using Unity.Mathematics;
using UnityEngine;

namespace JRT.World.Light
{
    public class AreaLight : BaseLightNode
    {
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

            node.Sampler = Sampler.GetSamplerData();
            node.SampleArea = _CalculateLightArea() / node.Sampler.SampleCount;

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

        public float3[] GenerateWorldSamplingPoints()
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
    }
}
