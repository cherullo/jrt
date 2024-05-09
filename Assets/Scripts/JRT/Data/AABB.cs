using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace JRT.Data
{
    public struct AABB
    {
        private static readonly bool3[] Corners = new bool3[]
        {
            new bool3(false, false, false),
            new bool3(true, false, false),
            new bool3(false, true, false),
            new bool3(true, true, false),
            new bool3(false, false, true),
            new bool3(true, false, true),
            new bool3(false, true, true),
            new bool3(true, true, true),
        };

        private static readonly int2[] Faces = new int2[]
        {
            new int2(1, 7), // X+
            new int2(0, 6), // X-
            new int2(2, 7), // Y+
            new int2(0, 5), // Y-
            new int2(4, 7), // Z+
            new int2(0, 3), // Z-
        };

        private static readonly float4[] Normals = new float4[] 
        {
            new(1,  0,  0, 0),
            new(-1, 0,  0, 0),
            new(0,  1,  0, 0),
            new(0, -1,  0, 0),
            new(0,  0,  1, 0),
            new(0,  0, -1, 0),
        };

        public float3 Min, Max;

        public static readonly AABB Empty = new(float.MaxValue, float.MinValue);

        public AABB(float4 min, float4 max)
        {
            Min = min.xyz;
            Max = max.xyz;
        }

        public AABB(float3 min, float3 max)
        {
            Min = min;
            Max = max;
        }

        public AABB(UnityEngine.Bounds bounds)
        {
            Min = bounds.min;
            Max = bounds.max;
        }

        public AABB(float min, float max) : this()
        {
            Min = new(min, min, min);
            Max = new(max, max, max);
        }

        private static readonly ShuffleComponent[] BitmaskToUVShuffle = new ShuffleComponent[] {
            ShuffleComponent.LeftZ, ShuffleComponent.LeftY, // 0b0000
            ShuffleComponent.LeftZ, ShuffleComponent.LeftY, // 0b0001
            ShuffleComponent.LeftX, ShuffleComponent.LeftZ, // 0b0010
            ShuffleComponent.LeftZ, ShuffleComponent.LeftY, // 0b0011
            ShuffleComponent.LeftX, ShuffleComponent.LeftY, // 0b0100
            ShuffleComponent.LeftZ, ShuffleComponent.LeftY, // 0b0101
            ShuffleComponent.LeftX, ShuffleComponent.LeftZ, // 0b0110
            ShuffleComponent.LeftZ, ShuffleComponent.LeftY  // 0b0111
        };

        private static readonly ShuffleComponent[] BitmaskToXShuffle = new ShuffleComponent[] {
            ShuffleComponent.LeftZ, // 0b0000
            ShuffleComponent.LeftZ, // 0b0001
            ShuffleComponent.LeftX, // 0b0010
            ShuffleComponent.LeftZ, // 0b0011
            ShuffleComponent.LeftX, // 0b0100
            ShuffleComponent.LeftZ, // 0b0101
            ShuffleComponent.LeftX, // 0b0110
            ShuffleComponent.LeftZ, // 0b0111
        };

        private static readonly ShuffleComponent[] BitmaskToYShuffle = new ShuffleComponent[] {
            ShuffleComponent.LeftY, // 0b0000
            ShuffleComponent.LeftY, // 0b0001
            ShuffleComponent.LeftZ, // 0b0010
            ShuffleComponent.LeftY, // 0b0011
            ShuffleComponent.LeftY, // 0b0100
            ShuffleComponent.LeftY, // 0b0101
            ShuffleComponent.LeftZ, // 0b0110
            ShuffleComponent.LeftY  // 0b0111
        };

        private static readonly float3[] BitmaskToNormal = new float3[] {
            new float3(1, 0, 0),    // 0b0000
            new float3(1, 0, 0),    // 0b0001
            new float3(0, 1, 0),    // 0b0010
            new float3(1, 0, 0),    // 0b0011
            new float3(0, 0, 1),    // 0b0100
            new float3(1, 0, 0),    // 0b0101
            new float3(0, 1, 0),    // 0b0110
            new float3(1, 0, 0),    // 0b0111
        };

        public bool IsIntersectedBy(in Ray ray, out HitPoint hitPoint)
        {
            float3 invDir = 1.0f / ray.Direction.xyz;

            float3 t1 = (Min - ray.Start.xyz) * invDir;
            float3 t2 = (Max - ray.Start.xyz) * invDir;

            float3 m = math.min(t1, t2);
            float3 M = math.max(t1, t2);

            float tMin = math.cmax(m);
            float tMax = math.cmin(M);

            if ((tMin > tMax) || (tMax < 0.0f))
            {
                hitPoint = HitPoint.Invalid;
                return false;
            }

            hitPoint.FrontHit = (tMin >= 0);
            float t = (hitPoint.FrontHit) ? tMin : tMax;
            hitPoint.Point = ray.Start + t * ray.Direction;
            hitPoint.T = t;

            float3 pointAbs = math.abs(hitPoint.Point.xyz);
            float pointMax = math.cmax(pointAbs);
            bool3 boolVector = (pointAbs == pointMax);

            int bitmask = math.bitmask(new bool4(boolVector, false));

            hitPoint.Normal = math.sign(hitPoint.Point.xyz) * BitmaskToNormal[bitmask];
            hitPoint.TexCoords = 0.5f + math.shuffle(hitPoint.Point, -hitPoint.Point, BitmaskToXShuffle[bitmask], BitmaskToYShuffle[bitmask]);

            return true;
        }

        public bool IsIntersectedByFast(in RayInvDir ray, ref HitPoint hitPoint)
        {
            float3 t1 = (Min - ray.Start) * ray.Direction;
            float3 t2 = (Max - ray.Start) * ray.Direction;

            float3 m = math.min(t1, t2);
            float3 M = math.max(t1, t2);

            float tMin = math.cmax(m);
            float tMax = math.cmin(M);

            if ((tMin > tMax) || (tMax < 0.0f))
                return false;

            hitPoint.FrontHit = (tMin >= 0);
            hitPoint.T = (hitPoint.FrontHit) ? tMin : tMax;

            return true;
        }

        public AABB Transform(in float4x4 matrix)
        {
            float4 retMin = float.MaxValue;
            float4 retMax = float.MinValue;

            for (int i = 0; i < Corners.Length; i++)
            {
                float4 localVertex = new float4(GetCorner(i), 1.0f);
                float4 transformedVertex = math.mul(matrix, localVertex);
                retMin = math.min(retMin, transformedVertex);
                retMax = math.max(retMax, transformedVertex);
            }

            return new AABB(retMin, retMax);
        }

        public float3 GetCorner(int i)
        {
            return math.select(Min, Max, Corners[i]);
        }

        public void Encapsulate(AABB other)
        {
            Min = math.min(Min, other.Min);
            Max = math.max(Max, other.Max);
        }
    }
}
