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
            //newDirection.xyz = math.normalize(newDirection.xyz);

            return new Ray(
                math.mul(matrix, Start),
                newDirection
            );

            //float4 newStart = math.mul(matrix, Start);
            //float4 newDirection = math.mul(matrix, Start + Direction);
            //newDirection -= newStart;
            //newDirection.xyz = math.normalize(newDirection.xyz);

            //return new Ray(
            //    newStart,
            //    newDirection
            //);
        }
    }
}
