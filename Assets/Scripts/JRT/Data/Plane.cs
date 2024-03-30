using Unity.Mathematics;

namespace JRT.Data
{
    public struct Plane
    {
        public float4 Point;
        public float4 Normal;

        public Plane(float4 point, float4 normal)
        {
            Point = point;
            Normal = math.normalize(normal);
        }

        public Plane TransformToLocal(GeometryNode node)
        {
            return new Plane (
                math.mul(node.WorldToLocal, Point),
                math.mul(Normal, node.LocalToWorld) // transpose of the inverse
            );
        }

        public Plane TransformToWorld(GeometryNode node)
        {
            return new Plane(
                math.mul(node.LocalToWorld, Point),
                math.mul(Normal, node.WorldToLocal) // transpose of the inverse
            );
        }

        public bool IsIntersectedBy(in Ray ray, out HitPoint hitPoint)
        {
            float denom = math.dot(ray.Direction, Normal);
            if (math.abs(denom) < math.EPSILON)
            {
                hitPoint = HitPoint.Invalid;
                return false;
            }

            float t = math.dot(Point - ray.Start, Normal) / denom;

            if (t < 0)
            {
                hitPoint = HitPoint.Invalid;
                return false;
            }

            hitPoint = new HitPoint(ray.Start + t * ray.Direction, Normal, denom < 0.0f);
            return true;
        }
    }
}
