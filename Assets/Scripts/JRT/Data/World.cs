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

        public int Depth;

        public float3 TraceRay(Ray ray) 
        {
            if (Depth > 10)
                return 0;

            int hitIndex = ComputeIntersection(ray, out HitPoint hitPoint);
            
            if (hitIndex == -1) 
                return CalculateMissColor(ray);

            if (Geometries[hitIndex].IsLightGeometry() == true)
            {
                int lightIndex = Geometries[hitIndex].LightIndex;
                return CalculateDirectLightColor(ray, hitPoint, lightIndex);
            }

            Depth++;
            float3 ret = Geometries[hitIndex].Material.CalculateColor(ref this, ray, hitPoint);
            Depth--;
            return ret;
        }

        private float3 CalculateDirectLightColor(Ray ray, HitPoint hitPoint, int lightIndex)
        {
            LightNode light = Lights[lightIndex];
            return light.Color;
                        
            //float distance = math.length(ray.Start - hitPoint.Point);
            
            //float3 color = light.Color * light.Power / (distance);

            //float max = math.cmax(color);

            //if (max > 1.0f)
            //    color /= max;

            //return color;
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

            RayInvDir invDir = ray.InvertDirection();
            
            HitPoint tempHitPoint = HitPoint.Invalid;
            for (int i = 0; i < Geometries.Length; i++)
            {
                GeometryNode node = Geometries[i];

                if (node.Bounds.IsIntersectedByFast(invDir, ref tempHitPoint) == false)
                    continue;

                if (tempHitPoint.FrontHit && (tempHitPoint.T >= hitPoint.T))
                    continue;

                if (node.IsIntersectedBy(ray, out tempHitPoint) == false)
                    continue;

                if ((tempHitPoint.T > 0.0001f) && (tempHitPoint.T < hitPoint.T))
                {
                    hitNodeIndex = i;
                    hitPoint = tempHitPoint;
                }
            }

            return hitNodeIndex;
        }
    }
}
