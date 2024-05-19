using JRT.Data;

namespace JRT.World.Light
{
    public class PointLight : BaseLightNode
    {
        public override LightType GetLightType()
        {
            return LightType.PointLight;
        }

        public override LightNode GetNodeData()
        {
            return GetBaseData();
        }
    }
}
