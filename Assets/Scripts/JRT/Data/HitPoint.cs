using Unity.Mathematics;

namespace JRT.Data
{
    public struct HitPoint
    {
        public float4 Point;
        public float4 Normal;
        public bool FrontHit;

        public HitPoint(float4 point, float4 normal, bool frontHit)
        {
            Point = point;
            Normal = normal;
            FrontHit = frontHit;
        }

        public HitPoint TransformToWorld(GeometryNode node)
        {
            return new HitPoint(
                math.mul(node.LocalToWorld, Point),
                new float4(math.normalize(math.mul(Normal, node.WorldToLocal).xyz), 0),  // transpose of the inverse
                FrontHit
            );
        }

        public bool IsValid()
        {
            return !Normal.Equals(float4.zero);
        }

        public static HitPoint Invalid => new HitPoint();
    }
}
