using Unity.Mathematics;

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

        public AABB(float min, float max) : this()
        {
            Min = new(min, min, min, 1.0f);
            Max = new(max, max, max, 1.0f);
        }

        public bool IsIntersectedBy(in Ray ray)
        {
            // Start is inside the AABB
            if ((Min < ray.Start).Equals(ray.Start < Max))
                return true;

            for (int i = 0; i < Faces.Length; i++)
            {
                float4 normal = Normals[i];
                // Don't check against the back face
                if (math.dot(normal, ray.Direction) > 0.0f)
                    continue;

                int2 face = Faces[i];
                float4 firstCorner = GetCorner(face.x);
                Plane plane = new Plane(firstCorner, normal);
                if (plane.IsIntersectedBy(ray, out HitPoint hitPoint) == false)
                    continue;

                float4 secondCorner = GetCorner(face.y);
                bool4 normalPlane = (normal != 0.0f);

                bool4 hitInsideFace = normalPlane | ((firstCorner < hitPoint.Point) & (hitPoint.Point < secondCorner));
                if (hitInsideFace.xyz.Equals(new bool3(true)) == true)
                    return true;
            }

            return false;
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
