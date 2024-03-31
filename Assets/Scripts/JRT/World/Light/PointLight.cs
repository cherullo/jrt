using JRT.Data;

namespace JRT.World.Light
{
    public class PointLight : BaseLightNode
    {
        public override Data.LightType GetLightType()
        {
            return Data.LightType.PointLight;
        }

        public override LightNode GetNodeData()
        {
            return base.GetBaseData();
        }
    }
}
