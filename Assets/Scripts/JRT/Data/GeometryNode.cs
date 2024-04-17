using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct GeometryNode
    {
        public int Index;
        public GeometryType Type;
        public int LightIndex;

        public AABB Bounds;
        public float4x4 WorldToLocal;
        public float4x4 LocalToWorld;

        public Material Material;

        public UnsafeList<Triangle> Triangles;

        public bool IsValid()
        {
            return Type != GeometryType.Undefined;
        }

        public bool IsLightGeometry()
        {
            return IsValid() && (LightIndex != -1);
        }

        public bool IsIntersectedBy(Ray ray, out HitPoint hitPoint)
        {
            bool result;
            Ray localRay = ray.TransformToLocal(this);

            switch (Type)
            {
                default:
                case GeometryType.Undefined:
                    hitPoint = HitPoint.Invalid;
                    result = false;
                    break;

                case GeometryType.Box:
                    result = new AABB(-0.5f, 0.5f).IsIntersectedBy(localRay, out hitPoint);
                    break;

                case GeometryType.Sphere:
                    result = _IntersectOriginCenteredSphere(0.5f, localRay, out hitPoint);
                    break;

                case GeometryType.Mesh:
                    result = _IntersectMesh(localRay, out hitPoint);
                    break;
            }

            if (result == true)
                hitPoint = hitPoint.TransformToWorld(this);

            return result;
        }

        private bool _IntersectMesh(Ray ray, out HitPoint resultingHitPoint)
        {
            resultingHitPoint = HitPoint.Invalid;
            float t = float.MaxValue;

            for (int i = 0; i < Triangles.Length; i++)
            {
                if (Triangles[i].IsIntersectedBy(ray, out HitPoint hitPoint) == true)
                {
                    if (hitPoint.T < t)
                    {
                        t = hitPoint.T;
                        resultingHitPoint = hitPoint;
                    }
                }
            }

            return (t != float.MaxValue);
        }

        [BurstCompile]
        private bool _IntersectOriginCenteredSphere(float radius, Ray ray, out HitPoint hitPoint)
        {
            float a = math.lengthsq(ray.Direction.xyz);
            float b = 2.0f * math.dot(ray.Direction.xyz, ray.Start.xyz);
            float c = math.lengthsq(ray.Start.xyz) - radius * radius;

            float delta = b * b - 4.0f * a * c;

            if (delta < 0)
            {
                hitPoint = HitPoint.Invalid;
                return false;
            }

            delta = math.sqrt(delta);
            float x1 = (-b - delta) / (2.0f * a);
            float x2 = (-b + delta) / (2.0f * a);

            if (x1 < 0.0f)
            {
                if (x2 < 0.0f)
                {
                    hitPoint = HitPoint.Invalid;
                    return false;
                }
                else
                {
                    float4 point = ray.Start + ray.Direction * x2;
                    float3 normal = 2.0f * point.xyz; // new float4(math.normalize(point.xyz), 0.0f);
                    hitPoint = new HitPoint(point, normal, x2, false);
                    return true;
                }
            }
            else
            {
                float4 point = ray.Start + ray.Direction * x1;
                float3 normal = 2.0f * point.xyz; // The same as normalization since point rests in a origin centered sphere of radius 0.5f
                hitPoint = new HitPoint(point, normal, x1, true);
                return true;
            }
        }

        public static GeometryNode Invalid {
            get
            {
                var ret = new GeometryNode();
                ret.LightIndex = -1;
                return ret;
            }
        }
    }
}
