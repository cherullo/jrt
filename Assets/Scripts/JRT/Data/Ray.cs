using Unity.Mathematics;

namespace JRT.Data
{
    public struct Ray
    {
        public float4 start;
        public float4 direction;

        public Ray(float4 start, float4 direction)
        {
            this.start = start;
            this.direction = direction;
        }

        public bool Intersects(AABB aabb)
        {
            return false;
        }

        public bool Intersects(GeometryNode node)
        {
            return Intersects(node.Bounds);
        }

        public Ray TransformToLocal(GeometryNode node)
        {
            return this;
        }

        public Ray TransformToWorld(GeometryNode node)
        {
            return this;
        }
    }
}
