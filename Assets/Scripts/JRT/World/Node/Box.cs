using JRT.Data;

namespace JRT.World.Node
{
    public class Box : BaseNode
    {
        public override GeometryNode GetData()
        {
            GeometryNode ret = GetBaseData();

            AABB localBounds = new(-0.5f, 0.5f);

            ret.Bounds = localBounds.Transform(ret.LocalToWorld);

            return ret;
        }

        public override GeometryType GetGeometryType()
        {
            return GeometryType.Box;
        }
    }
}
