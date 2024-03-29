using JRT.Data;

using UnityEngine;

namespace JRT.World.Node
{
    public abstract class BaseNode : MonoBehaviour, IGeometry
    {
        public abstract GeometryType GetGeometryType();

        public abstract GeometryNode GetData();

        protected GeometryNode GetBaseData()
        {
            GeometryNode ret = new GeometryNode();

            ret.LocalToWorld = transform.localToWorldMatrix;
            ret.WorldToLocal = transform.worldToLocalMatrix;
            ret.Type = GetGeometryType();

            return ret;
        }
    }
}
