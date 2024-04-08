using JRT.Data;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.World.Light
{
    public abstract class BaseLightNode : MonoBehaviour, ILight
    {
        public float Power = 10.0f;
        public Color Color;

        public abstract Data.LightType GetLightType();

        public abstract LightNode GetNodeData();

        protected LightNode GetBaseData()
        {
            return new LightNode()
            {
                Type = GetLightType(),
                Power = Power,
                Color = Color.ToFloat3(),
                Position = new float4(transform.position, 1.0f),
                SampleArea = 1.0f
            };
        }
    }
}
