using Unity.Mathematics;

namespace JRT.Data
{
    public struct Plane
    {
        public float4 Point;
        public float3 Normal;

        public Plane(float4 point, float3 normal)
        {
            Point = point;
            Normal = math.normalize(normal);
        }

        public Plane TransformToLocal(GeometryNode node)
        {
            return new Plane (
                math.mul(node.WorldToLocal, Point),
                math.mul(new float4(Normal, 0.0f), node.LocalToWorld).xyz // transpose of the inverse
            );
        }

        public Plane TransformToWorld(GeometryNode node)
        {
            return new Plane(
                math.mul(node.LocalToWorld, Point),
                math.mul(new float4(Normal, 0.0f), node.WorldToLocal).xyz // transpose of the inverse
            );
        }

        public bool IsIntersectedBy(in Ray ray, out HitPoint hitPoint)
        {
            float denom = math.dot(ray.Direction.xyz, Normal);
            if (math.abs(denom) < math.EPSILON)
            {
                hitPoint = HitPoint.Invalid;
                return false;
            }

            float t = math.dot((Point - ray.Start).xyz, Normal) / denom;

            if (t < 0)
            {
                hitPoint = HitPoint.Invalid;
                return false;
            }

            hitPoint = new HitPoint(ray.Start + t * ray.Direction, Normal, t, denom < 0.0f, 0.0f);
            return true;
        }
    }
}
