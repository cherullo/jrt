using Unity.Mathematics;

namespace JRT.Data
{
    public struct GeometryNode 
    {
        public GeometryType Type;

        public AABB Bounds;
        public float4x4 WorldToLocal;
        public float4x4 LocalToWorld;
    }
}
