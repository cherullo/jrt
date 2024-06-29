using System.Linq;
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

        public bool UseMIS;

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
            bool acceptDirectLightHit = true;

            for (int i = 0; i < TerminationProbabilities.Length; i++)
            {
                float terminationProbability = _CalculateTerminationProbability(i, beta);
                if (Random.float01 < terminationProbability)
                    break;
                beta /= (1.0f - terminationProbability);

                int hitIndex = ComputeIntersection(ray, out HitPoint hitPoint);
                if (hitIndex == -1)
                {
                    if (acceptDirectLightHit)
                        return beta * AmbientLight;

                    break;
                }

                Material mat = Geometries[hitIndex].Material;
                float4 point = hitPoint.Point;
                float3 normal = hitPoint.Normal;

                if (Geometries[hitIndex].IsLightGeometry() == true)
                {
                    if (acceptDirectLightHit == true) // First ray or reflection
                        return beta * CalculateDirectLightColor(ray, hitPoint, Geometries[hitIndex].LightIndex);

                    break;
                }

                // Perfect Mirror
                if (mat.Reflectance == 1.0f)
                { 
                    beta *= mat.GetDiffuseColor(hitPoint.TexCoords);

                    ray = new Ray(point, reflect(ray.Direction.xyz, normal));
                    acceptDirectLightHit = true;
                    continue;
                }

                // Direct light sampling using MIS
                ChooseRandomLight(out int lightIndex, out float lightProbability);
                Lights[lightIndex].ChooseRandomSample(ref Random, out int sampleIndex, out float sampleProbability);
                float light_pdf = lightProbability * sampleProbability;

                float3 Le = Lights[lightIndex].CalculateRadiance(ref this, point, normal, sampleIndex, out float4 lightPoint, out float3 lightDir);
                float3 pointToLightDir = normalize((lightPoint - point).xyz);
                Le *= max(0, dot(normal, pointToLightDir));

                float3 pointDiffuseColor = mat.GetDiffuseColor(hitPoint.TexCoords);
                float3 BRDF = mat.GetBRDF(pointDiffuseColor, pointToLightDir, normal, -ray.Direction.xyz);

                float mis_weight = 1.0f;
                if (UseMIS == true)
                {
                    float G = Utils.GeometricFactor(point, lightPoint, lightDir);
                    float material_pdf = mat.GetDirectionPDF(normal, pointToLightDir);
                    mis_weight = Utils.MIS_BalanceHeuristic(light_pdf, material_pdf * G);
                }

                L += Le * BRDF * beta * mis_weight / light_pdf;

                if (UseMIS == true)
                {
                    mat.GetHemisphereSample(ref Random, out float3 matDirection, out float matSampleProbability);
                    float3 matSampleDirection = new Hemisphere(point, normal).ToGlobal(matDirection);
                    // Cos-weighted light sampling using MIS
                    //float matSampleProbability = 1.0f;
                    //float3 matSampleDirection = normalize(reflect(ray.Direction.xyz, normal));

                    int sampleHitIndex = ComputeIntersection(new Ray(point, matSampleDirection), out HitPoint matHitPoint);
                    if (sampleHitIndex < 0)
                    {
                        L += beta * AmbientLight; // * weight??
                    }
                    else if (Geometries[sampleHitIndex].IsLightGeometry() == true)
                    {
                        int matLightIndex = Geometries[sampleHitIndex].LightIndex;
                        LightNode matLight = Lights[matLightIndex];

                        matLight.ChooseRandomSample(ref Random, out _, out light_pdf);
                        light_pdf *= GetLightProbability(matLightIndex);

                        var temp = matLight.GetLightDirection(point);
                        float G = Utils.GeometricFactor(point, matHitPoint.Point, temp);

                        mis_weight = Utils.MIS_BalanceHeuristic(matSampleProbability * G, light_pdf);

                        Le = matLight.Color * matLight.Power * G / (matLight.SampleArea * matLight.GetSampleCount());

                        BRDF = mat.GetBRDF(pointDiffuseColor, matSampleDirection, normal, -ray.Direction.xyz);

                        L += beta * BRDF *  mis_weight * Le / matSampleProbability;
                    }
                }

                // Create new direction for path
                

                float3 newDirection;
                float3 rout = reflect(ray.Direction.xyz, normal);
                float hemSampleProbability;
                do
                {
                    mat.GetHemisphereSample(ref Random, out float3 hemDirection, out hemSampleProbability);

                    if (mat.Type == MaterialType.Microfacet)
                    {
                        hemDirection.xy = hemDirection.xy * mat.MicrofacetData.Roughness;
                        hemDirection = normalize(hemDirection);
                    }

                    newDirection = new Hemisphere(point, rout).ToGlobal(hemDirection);
                } while (dot(newDirection, normal) < 0.05f);
                //hemSampleProbability = dot(normal, rout);
                //newDirection = rout;

                // float3 newDirection = new Hemisphere(point, normal).ToGlobal(hemDirection);
                beta *= mat.GetBRDF(pointDiffuseColor, newDirection, normal, -ray.Direction.xyz) * max(0, dot(normal, newDirection)) / hemSampleProbability;
                //beta *= mat.GetBRDF(pointDiffuseColor, pointToLightDir, normal, -ray.Direction.xyz) * max(0, dot(normal, newDirection)) / hemSampleProbability;

                ray = new Ray(point, newDirection);
                acceptDirectLightHit = false;
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
                return max(tableProbability, min(0.95f,  1.0f - beta.Luminance()));
                //return max(tableProbability, 1.0f - beta.Luminance());
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

        private float GetLightProbability(int lightIndex)
        {
            float allPower = 0.0f;

            for (int i = 0; i < Lights.Length; i++)
                allPower += Lights[i].Power;

            return Lights[lightIndex].Power / allPower;
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
