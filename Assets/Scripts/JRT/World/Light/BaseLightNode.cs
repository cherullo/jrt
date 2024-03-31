using JRT.Data;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.World.Light
{
    public abstract class BaseLightNode : MonoBehaviour, ILight
    {
        public float3 Color;

        public abstract Data.LightType GetLightType();

        public abstract LightNode GetNodeData();

        public LightNode GetBaseData()
        {
            return new LightNode()
            {
                Type = GetLightType(),
                Color = new float4(Color, 1.0f),
                Position = new float4(transform.position, 0.0f)
            };
        }
    }
}