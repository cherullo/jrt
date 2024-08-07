using Unity.Mathematics;

namespace JRT.Data
{
    public struct Triangle 
    {
        public float3 P0; 
        public float3 P1; 
        public float3 P2;
        public float3 N0;
        public float3 N1;
        public float3 N2;
        public float2 Tex0;
        public float2 Tex1;
        public float2 Tex2;

        public bool IsIntersectedBy(in Ray ray, ref HitPoint hitPoint)
        {
            //hitPoint = HitPoint.Invalid;

            float3 r = ray.Start.xyz - P0;
            float3 e1 = P1 - P0;
            float3 e2 = P2 - P0;

            float3 e2xe1 = math.cross(e2, e1);
            float denom = math.dot(ray.Direction.xyz, math.cross(e2, e1));
            if (math.abs(denom) < float.Epsilon)
                return false;

            float invDenom = 1.0f / denom;

            float u = invDenom * math.dot(ray.Direction.xyz, math.cross(e2, r));
            if ((u < 0.0f) || (u > 1.0f)) return false;

            float v = invDenom * math.dot(ray.Direction.xyz, math.cross(r, e1));
            if ((v < 0.0f) || (v > 1.0f)) return false;

            if ((u + v) > 1.0f)
                return false;

            //float t = invDenom * math.dot(r, math.cross(e1, e2));
            float t = invDenom * math.dot(r, e2xe1);
            if (t > 0.0f) return false;

            hitPoint.FrontHit = (denom > 0.0f);
            hitPoint.Point = ray.Start + t * ray.Direction;
            hitPoint.Normal = math.normalize((1.0f - u - v) * N0 + u * N1 + v * N2);
            hitPoint.TexCoords = (1.0f - u - v) * Tex0 + u * Tex1 + v * Tex2;
            hitPoint.T = -t;
            return true;
        }

        public bool IsIntersectedByFast(in Ray ray, ref FastTriHit fastHit)
        {
            float3 r = ray.Start.xyz - P0;
            float3 e1 = P1 - P0;
            float3 e2 = P2 - P0;

            float3 e2xe1 = math.cross(e2, e1);
            float denom = math.dot(ray.Direction.xyz, e2xe1);
            if (math.abs(denom) < float.Epsilon)
                return false;

            float u = math.dot(ray.Direction.xyz, math.cross(e2, r));
            if ((u < 0.0f) || (u > denom)) return false;

            float v = math.dot(ray.Direction.xyz, math.cross(r, e1));
            if ((v < 0.0f) || (v > denom)) return false;

            if ((u + v) > denom)
                return false;

            float t = math.dot(r, e2xe1) / denom;
            if (t > 0.0f) return false;

            fastHit = new FastTriHit(-t, u, v, denom);
            
            return true;
        }

        public void CalculateHitDetails(Ray ray, float t, out HitPoint hitPoint)
        {
            float3 r = ray.Start.xyz - P0;
            float3 e1 = P1 - P0;
            float3 e2 = P2 - P0;

            float denom = math.dot(ray.Direction.xyz, math.cross(e2, e1));

            float invDenom = 1.0f / denom;

            float u = invDenom * math.dot(ray.Direction.xyz, math.cross(e2, r));
            float v = invDenom * math.dot(ray.Direction.xyz, math.cross(r, e1));

            hitPoint.FrontHit = (denom > 0.0f);
            hitPoint.Point = ray.Start + (t * 0.999f) * ray.Direction;
            hitPoint.Normal = math.normalize((1.0f - u - v) * N0 + u * N1 + v * N2);
            hitPoint.TexCoords = (1.0f - u - v) * Tex0 + u * Tex1 + v * Tex2;
            hitPoint.T = t * 0.999f;
        }

        public void CalculateHitDetails(Ray ray, FastTriHit fastHit, out HitPoint hitPoint)
        {
            float invDenom = 1.0f / fastHit.Denom;

            float u = invDenom * fastHit.U;
            float v = invDenom * fastHit.V;

            hitPoint.FrontHit = (fastHit.Denom > 0.0f);
            hitPoint.Point = ray.Start + fastHit.T * 0.999f * ray.Direction;
            hitPoint.Normal = math.normalize((1.0f - u - v) * N0 + u * N1 + v * N2);
            hitPoint.TexCoords = (1.0f - u - v) * Tex0 + u * Tex1 + v * Tex2;
            hitPoint.T = fastHit.T * 0.999f;
        }

        public AABB CalculateAABB()
        {
            return new AABB(
                math.min(math.min(P0, P1), P2),
                math.max(math.max(P0, P1), P2)
            );
        }
    }

    public struct FastTriHit
    {
        private float4 _data;

        public FastTriHit(float t, float u, float v, float denom)
        {
            _data = new float4(t, u, v, denom);
        }

        public float T { get => _data.x; }
        public float U { get => _data.y; }
        public float V { get => _data.z; }
        public float Denom { get => _data.w; }

        public static FastTriHit Invalid { get => new FastTriHit(0, 0, 0, 0); }
    }
}
