using Unity.Mathematics;

namespace JRT.Data
{
    public struct Ray
    {
        public float4 Start;
        public float4 Direction;

        public Ray(float4 start, float4 direction)
        {
            Start = start;
            Direction = direction;
        }

        public Ray(float4 start, float3 direction) : this(start, new float4(direction, 0))
        {
        }

        public Ray TransformToLocal(GeometryNode node)
        {
            return Transform(node.WorldToLocal);
        }

        public Ray TransformToWorld(GeometryNode node)
        {
            return Transform(node.LocalToWorld);
        }

        private Ray Transform(float4x4 matrix)
        {
            float4 newDirection = math.mul(matrix, Direction);

            return new Ray(
                math.mul(matrix, Start),
                newDirection
            );
        }

        public RayInvDir InvertDirection()
        {
            return new RayInvDir(
                Start.xyz,
                1.0f / Direction.xyz
            );
        }
    }

    public struct RayInvDir
    {
        public float3 Start;
        public float3 Direction;

        public RayInvDir(float3 start, float3 direction)
        {
            Start = start;
            Direction = direction;
        }
    }
}
