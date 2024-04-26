using Unity.Collections;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct World 
    {
        public RNG Random;

        public float3 AmbientLight;

        [ReadOnly]
        public NativeArray<GeometryNode> Geometries;

        [ReadOnly]
        public NativeArray<LightNode> Lights;

        public float3 TraceRay(Ray ray) 
        {
            int hitIndex = ComputeIntersection(ray, out HitPoint hitPoint);

            if (hitIndex == -1)
                return CalculateMissColor(ray);

            if (Geometries[hitIndex].IsLightGeometry() == true)
            {
                int lightIndex = Geometries[hitIndex].LightIndex;
                return CalculateDirectLightColor(ray, hitPoint, lightIndex);
            }

            return Geometries[hitIndex].Material.CalculateColor(ref this, ray, hitPoint);
        }

        private float3 CalculateDirectLightColor(Ray ray, HitPoint hitPoint, int lightIndex)
        {
            float distance = math.length(ray.Start - hitPoint.Point);

            LightNode light = Lights[lightIndex];

            return light.Color * light.Power;// / (distance));
        }

        private float3 CalculateMissColor(Ray ray)
        {
            return float3.zero;
        }

        public int ComputeIntersection(Ray ray, out HitPoint hitPoint)
        {
            int hitNodeIndex = -1;
            hitPoint = HitPoint.Invalid;
            hitPoint.T = float.MaxValue;

            for (int i = 0; i < Geometries.Length; i++)
            {
                GeometryNode node = Geometries[i];

                if (node.Bounds.IsIntersectedBy(ray, out _) == false)
                    continue;

                if (node.IsIntersectedBy(ray, out HitPoint tempHitPoint) == false)
                    continue;

                // Avoid self intersection
                //if ((tempHitPoint.FrontHit || tempHitPoint.T > 0.001f) && (tempHitPoint.T < hitPoint.T))
                if ((tempHitPoint.FrontHit) && (tempHitPoint.T < hitPoint.T))
                {
                    hitNodeIndex = i;
                    hitPoint = tempHitPoint;
                }
            }

            return hitNodeIndex;
        }
    }
}
