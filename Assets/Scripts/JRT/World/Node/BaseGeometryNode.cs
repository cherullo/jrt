using JRT.Data;
using JRT.World.Light;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.World.Node
{
    public abstract class BaseGeometryNode : MonoBehaviour, IGeometry
    {
        public float3 Color = 1.0f;
        public float3 SpecularColor = 1.0f;
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
            ret.Material.DiffuseColor = Color;
            ret.Material.SpecularColor = SpecularColor;
            ret.Material.Shininess = Shininess;

            return ret;
        }
    }
}
