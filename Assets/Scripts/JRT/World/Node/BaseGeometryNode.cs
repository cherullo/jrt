using JRT.Data;
using JRT.World.Light;

using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.World.Node
{
    public abstract class BaseGeometryNode : MonoBehaviour, IGeometry
    {
        public MaterialType MaterialType;
        public Color DiffuseColor = Color.white;
        public Color SpecularColor = Color.white;
        public float Shininess = 1.0f;
        public float Reflectance = 0.0f;

        public BaseLightNode Light;

        private UnsafeList<float3> _diffuseColor;

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

            return ret;
        }

        private Data.Texture _GetDiffuseTexture()
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
            int pixelCount = texture.width * texture.height;

            ret.Pixels = new UnsafeList<float3>(pixelCount, Unity.Collections.AllocatorManager.Persistent);
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < pixelCount; i++)
                ret.Pixels.AddNoResize(new float3(colors[i].r, colors[i].g, colors[i].b));

            return ret;
        }

        private void OnDestroy()
        {
            _diffuseColor.Dispose();
        }
    }
}
