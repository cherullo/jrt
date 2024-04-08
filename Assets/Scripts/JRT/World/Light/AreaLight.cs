using JRT.Data;
using JRT.Sampling;
using UnityEngine;

namespace JRT.World.Light
{
    public class AreaLight : BaseLightNode
    {
        [SerializeField]
        private int _columns;

        [SerializeField]
        private int _rows;

        [SerializeField]
        private MultiSamplingType _samplingType;

        public override Data.LightType GetLightType()
        {
            return Data.LightType.AreaLight;
        }

        public override LightNode GetNodeData()
        {
            LightNode node = GetBaseData();
            
            return node;
        }
    }
}
