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

        public bool IsValid()
        {
            return Normal.Equals(float4.zero);
        }
    }
}
