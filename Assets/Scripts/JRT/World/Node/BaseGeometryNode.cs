using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

using JRT.Data;
using JRT.Utils;
using JRT.World.Light;

namespace JRT.World.Node
{
    public abstract class BaseGeometryNode : MonoBehaviour, IGeometry
    {
        public MaterialType MaterialType;
        public Color DiffuseColor = Color.white;
        public Color SpecularColor = Color.white;
        public float Shininess = 1.0f;
        public float Reflectance = 0.0f;
        public MicrofacetData MicrofacetData;
        public BaseLightNode Light;

        private UnsafeList<Color> _diffuseColor;

        public abstract GeometryType GetGeometryType();

        public abstract GeometryNode GetNodeData();

        protected GeometryNode GetBaseData()
        {
            GeometryNode ret = new GeometryNode();

            ret.LocalToWorld = transform.localToWorldMatrix;
            ret.WorldToLocal = transform.worldToLocalMatrix;
            ret.Type = GetGeometryType();
            
            ret.Material = new Data.Material();
            ret.Material.Type = MaterialType;
            ret.Material.DiffuseColor = DiffuseColor.ToFloat3();
            ret.Material.SpecularColor = SpecularColor.ToFloat3();
            ret.Material.Shininess = Shininess;
            ret.Material.Reflectance = Reflectance;
            ret.Material.DiffuseTexture = _GetDiffuseTexture();
            ret.Material.MicrofacetData = MicrofacetData;

            return ret;
        }

        unsafe private Data.Texture _GetDiffuseTexture()
        {
            UnityEngine.Renderer renderer = GetComponent<UnityEngine.Renderer>();
            if (renderer == null)
                return Data.Texture.Invalid;

            Texture2D texture = renderer.sharedMaterial.mainTexture as Texture2D;
            if (texture == null)
                return Data.Texture.Invalid;

            Data.Texture ret = new Data.Texture();
            ret.Width = texture.width;
            ret.Height = texture.height;

            _diffuseColor = texture.GetPixels().ToUnsafeList();

            ret.Pixels = _diffuseColor;

            return ret;
        }

        protected virtual void OnDestroy()
        {
            _diffuseColor.Dispose();
        }
    }
}
