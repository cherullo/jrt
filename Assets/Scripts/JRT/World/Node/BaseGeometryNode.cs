using JRT.Data;
using JRT.World.Light;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.World.Node
{
    public abstract class BaseGeometryNode : MonoBehaviour, IGeometry
    {
        public Color DiffuseColor = Color.white;
        public Color SpecularColor = Color.white;
        public Color AmbientColor = Color.white;
        public float Shininess = 1.0f;

        public BaseLightNode Light;

        public abstract GeometryType GetGeometryType();

        public abstract GeometryNode GetNodeData();

        protected GeometryNode GetBaseData()
        {
            GeometryNode ret = new GeometryNode();

            ret.LocalToWorld = transform.localToWorldMatrix;
            ret.WorldToLocal = transform.worldToLocalMatrix;
            ret.Type = GetGeometryType();
            
            ret.Material = new Data.Material();
            ret.Material.DiffuseColor = DiffuseColor.ToFloat3();
            ret.Material.SpecularColor = SpecularColor.ToFloat3();
            ret.Material.AmbientColor = AmbientColor.ToFloat3();
            ret.Material.Shininess = Shininess;

            return ret;
        }
    }
}
