using JRT.Data;

using UnityEngine;

namespace JRT.World.Light
{
    public abstract class BaseLightNode : MonoBehaviour, ILight
    {
        public float Power = 10.0f;
        public Color Color = Color.white;

        public abstract Data.LightType GetLightType();

        public abstract LightNode GetNodeData();

        protected LightNode GetBaseData()
        {
            return new LightNode()
            {
                Type = GetLightType(),
                Power = Power,
                Color = Color.ToFloat3(),
                LocalToWorld = transform.localToWorldMatrix,
                SampleArea = 1.0f
            };
        }
    }
}
