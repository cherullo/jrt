using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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

        [ReadOnly]
        public UnsafeList<float> TerminationProbabilities;

        public int MaxDepth;

        public int Depth;

        public bool TerminateBasedOnLuminance;

        public float3 TraceRay(Ray ray)
        {
            if (Depth > MaxDepth) 
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

        public float3 TracePath(Ray ray)
        {
            float3 L = 0;
            float3 beta = 1;

            for (int i = 0; i < TerminationProbabilities.Length; i++)
            {
                float terminationProbability = _CalculateTerminationProbability(i, beta);
                if (Random.float01 < terminationProbability)
                    break;
                beta /= (1.0f - terminationProbability);

                int hitIndex = ComputeIntersection(ray, out HitPoint hitPoint);
                if (hitIndex == -1)
                {
                    L += beta * AmbientLight;
                    break;
                }

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

                ChooseRandomLight(out int lightIndex, out float lightProbability);
                Lights[lightIndex].ChooseRandomSample(ref Random, out int sampleIndex, out float sampleProbability);

                float3 Le = Lights[lightIndex].CalculateRadiance(ref this, point, sampleIndex, out float3 pointToLightDir);
                Le *= max(0, dot(normal, pointToLightDir)) / (lightProbability * sampleProbability);

                float3 pointDiffuseColor = mat.GetDiffuseColor(hitPoint.TexCoords);
                float3 BRDF = pointDiffuseColor * mat.GetBRDF(normal, pointToLightDir);
                L += (Le * BRDF * beta);// / (1.0f - terminationProbability);

                mat.GetHemisphereSample(ref Random, out float3 hemDirection, out float hemSampleProbability);
                float3 newDirection = new Hemisphere(point, normal).ToGlobal(hemDirection);
                beta *= pointDiffuseColor * mat.GetBRDF(normal, newDirection) * max(0, dot(normal, newDirection)) / hemSampleProbability;

                ray = new Ray(point, newDirection);
            }

            return L;
        }

        private float _CalculateTerminationProbability(int depth, float3 beta)
        {
            float tableProbability = TerminationProbabilities[depth];

            if (tableProbability == 0.0f)
                return 0.0f;

            if (TerminateBasedOnLuminance == true)
            {
                return max(tableProbability, 1.0f - beta.Luminance());
            }

            return tableProbability;
        }

        private void ChooseRandomLight(out int lightIndex, out float lightProbability)
        {
            float random = Random.float01;
            float last = 0.0f;
            int i;
            for (i = 0; i < Lights.Length - 1; i++)
            {
                if (random < Lights[i].NormalizedAccumulatedPower)
                    break;

                last = Lights[i].NormalizedAccumulatedPower;
            }

            lightIndex = i;
            lightProbability = Lights[i].NormalizedAccumulatedPower - last;
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
