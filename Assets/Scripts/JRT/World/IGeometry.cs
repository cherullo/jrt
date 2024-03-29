using JRT.Data;

namespace JRT.World
{
    public interface IGeometry 
    {
        GeometryType GetGeometryType();
        GeometryNode GetData();
    }
}
