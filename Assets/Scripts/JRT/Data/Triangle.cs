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

        public bool IsIntersectedBy(in Ray ray, out HitPoint hitPoint)
        {
            hitPoint = HitPoint.Invalid;

            float3 r = ray.Start.xyz - P0;
            float3 e1 = P1 - P0;
            float3 e2 = P2 - P0;

            // ray.Start + t * ray.Direction = P0 + u * e1 + v * e2;
            // r = -t * ray.Direction + u * e1 + v * e2

            float denom = math.dot(ray.Direction.xyz, math.cross(e2, e1));
            if (denom == 0)
                return false;

            float invDenom = 1.0f / denom;
            
            hitPoint.FrontHit = (denom > 0.0f);

            float u = invDenom * math.dot(ray.Direction.xyz, math.cross(e2, r));
            if ((u < 0.0f) || (u > 1.0f)) return false;
            
            float v = invDenom * math.dot(ray.Direction.xyz, math.cross(r, e1));
            if ((v < 0.0f) || (v > 1.0f)) return false;

            if ((u + v) > 1.0f)
                return false;

            float t = invDenom * math.dot(r, math.cross(e1, e2));
            if (t < 0.0f) return false;

            hitPoint.Point = ray.Start + t * ray.Direction;
            hitPoint.Normal = math.normalize( (1.0f - u - v) * N0 + u * N1 + v * N2 );
            hitPoint.TexCoords = (1.0f - u - v) * Tex0 + u * Tex1 + v * Tex2;
            hitPoint.T = t;
            return true;
        }
    }
}
