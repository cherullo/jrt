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

        public float4 Min, Max;

        public AABB(float4 min, float4 max)
        {
            Min = min;
            Max = max;
        }

        public AABB(UnityEngine.Bounds bounds)
        {
            Min = new float4(bounds.min, 1.0f);
            Max = new float4(bounds.max, 1.0f);
        }

        public AABB(float min, float max) : this()
        {
            Min = new(min, min, min, 1.0f);
            Max = new(max, max, max, 1.0f);
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

        unsafe public bool IsIntersectedBy(in Ray ray, out HitPoint hitPoint)
        {
            float3 invDir = 1.0f / ray.Direction.xyz;

            float3 t1 = (Min.xyz - ray.Start.xyz) * invDir;
            float3 t2 = (Max.xyz - ray.Start.xyz) * invDir;

            float3 m = math.min(t1, t2);
            float3 M = math.max(t1, t2);

            float tMin = math.cmax(m);
            float tMax = math.cmin(M);

            if ((tMin >= tMax) || (tMax < 0.0f))
            {
                hitPoint = HitPoint.Invalid;
                return false;
            }

            hitPoint.FrontHit = (tMin >= 0);
            float t = (hitPoint.FrontHit) ? tMin : tMax;
            hitPoint.Point = ray.Start + t * ray.Direction;
                        
            float3 temp = math.abs(hitPoint.Point.xyz);
            float plane = math.cmax(temp);
            bool3 normalDirection = temp == plane;
            // bool4 uvSelector = new bool4(!normalDirection, false);
            int bmask = math.bitmask(new bool4(normalDirection, false));

            // hitPoint.Normal = math.select(0.0f, math.sign(hitPoint.Point.xyz), normalDirection);
            hitPoint.Normal = math.sign(hitPoint.Point.xyz) * BitmaskToNormal[bmask];

            hitPoint.TexCoords = 0.5f + math.shuffle(hitPoint.Point, -hitPoint.Point, BitmaskToUVShuffle[2 * bmask], BitmaskToUVShuffle[2 * bmask + 1]);
            // float4 texCoords = 0;
            //math.compress(&texCoords.x, 0, hitPoint.Point, uvSelector);
            //hitPoint.TexCoords = 0.5f + texCoords.yx;
            hitPoint.T = t;
            
            return true;
        }

        public AABB Transform(in float4x4 matrix)
        {
            float4 retMin = float.MaxValue;
            float4 retMax = float.MinValue;

            for (int i = 0; i < Corners.Length; i++)
            {
                float4 localVertex = GetCorner(i);
                float4 transformedVertex = math.mul(matrix, localVertex);
                retMin = math.min(retMin, transformedVertex);
                retMax = math.max(retMax, transformedVertex);
            }

            return new AABB(retMin, retMax);
        }

        public float4 GetCorner(int i)
        {
            return math.select(Min, Max, new bool4(Corners[i], false));
        }
    }
}
