using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;

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

        public float3 TracePath(Ray ray, int maxDepth)
        {
            float3 L = 0;
            float3 beta = 1;

            for (int i = 0; i < maxDepth; i++)
            {
                int hitIndex = ComputeIntersection(ray, out HitPoint hitPoint);

                if (hitIndex == -1)
                    break;

                if (Geometries[hitIndex].IsLightGeometry() == true)
                {
                    if (i == 0) // First ray
                        return CalculateDirectLightColor(ray, hitPoint, Geometries[hitIndex].LightIndex);
                    else
                        break;
                }

                Material mat = Geometries[hitIndex].Material;
                float4 point = hitPoint.Point;
                float3 normal = hitPoint.Normal;

                // TODO: Choose a light source and sample
                int lightIndex = 0;
                int sampleIndex = 0;
                float lightProbability = 1;
                float sampleProbability = 1;
                float3 Le = Lights[lightIndex].CalculateRadiance(ref this, point, sampleIndex, out float3 pointToLightDir);
                Le *= max(0, dot(normal, pointToLightDir)) / (lightProbability * sampleProbability);

                // TODO: Implement proper material BRDF
                float3 BRDF = mat.GetBRDF(normal, pointToLightDir);
                L += (Le * BRDF * beta);

                mat.GetHemisphereSample(ref Random, out float3 hemDirection, out float hemSampleProbability);
                float3 newDirection = new Hemisphere(point, normal).ToGlobal(hemDirection);
                beta *= mat.GetBRDF(normal, newDirection) * max(0, dot(normal, newDirection)) / hemSampleProbability;

                ray = new Ray(point, newDirection);
            }

            return L;
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
            return 0;
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

            hitPoint.Point = ray.Start + (hitPoint.T - 0.001f) * ray.Direction;

            return hitNodeIndex;
        }
    }
}
