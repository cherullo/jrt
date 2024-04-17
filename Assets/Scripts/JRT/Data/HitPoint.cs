using Unity.Mathematics;

namespace JRT.Data
{
    public struct HitPoint
    {
        public float4 Point;
        public float3 Normal;
        public float T;
        public bool FrontHit;
        public float2 TexCoords;

        public HitPoint(float4 point, float3 normal, float t, bool frontHit, float2 texCoords)
        {
            Point = point;
            Normal = normal;
            T = t;
            FrontHit = frontHit;
            TexCoords = texCoords;
        }

        public HitPoint TransformToWorld(GeometryNode node)
        {
            // Multiply by transpose of the inverse
            float3 transformedNormal = math.normalize(math.mul(new float4(Normal, 0.0f), node.WorldToLocal).xyz);

            return new HitPoint(
                math.mul(node.LocalToWorld, Point),
                transformedNormal,
                T,                
                FrontHit,
                TexCoords
            );
        }

        public bool IsValid()
        {
            return !Normal.Equals(float3.zero);
        }

        public static HitPoint Invalid => new HitPoint();
    }
}
