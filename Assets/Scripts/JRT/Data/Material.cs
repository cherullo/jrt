using Unity.Mathematics;

using static Unity.Mathematics.math;

namespace JRT.Data
{
    public struct Material
    {
        public MaterialType Type;
        public float3 DiffuseColor;
        public float3 SpecularColor;
        public float Shininess;
        public float Reflectance;
        public Texture DiffuseTexture;

        public float3 CalculateColor(ref World world, Ray ray, HitPoint hitPoint)
        {
            if (hitPoint.FrontHit == false)
                return new float3(1, 0, 0);

            switch (Type)
            {
                default:
                case MaterialType.Phong:
                    return CalculatePhongColor(ref world, ray, hitPoint);
                    
                case MaterialType.ReflectivePhong:
                    return CalculateReflectivePhongColor(ref world, ray, hitPoint);
            }
        }

        public float3 CalculateReflectivePhongColor(ref World world, Ray ray, HitPoint hitPoint)
        {
            float3 pointToEyeDir = math.normalize((ray.Start - hitPoint.Point).xyz);

            // float R = Reflectance + (1.0f - Reflectance) * math.pow(1.0f - math.dot(pointToEyeDir, hitPoint.Normal), 5.0f);
            float R = Reflectance + (1.0f - Reflectance) * math.pow(1.0f - math.saturate(math.dot(pointToEyeDir, hitPoint.Normal)), 5.0f);

            float3 color = (1.0f - R) * CalculatePhongColor(ref world, ray, hitPoint);

            float3 reflect = math.normalize(math.reflect(-pointToEyeDir, hitPoint.Normal));

            if (dot(reflect, hitPoint.Normal.xyz) > 0.000001f)
                color += R * world.TraceRay(new Ray(hitPoint.Point, reflect));

            return color;
        }

        public float3 CalculatePhongColor(ref World world, Ray ray, HitPoint hitPoint)
        {
            float3 diffuseColor = GetDiffuseColor(hitPoint.TexCoords);
            float3 color = diffuseColor * world.AmbientLight;
            float4 pointToEyeDir = new float4(math.normalize((ray.Start - hitPoint.Point).xyz), 0.0f);

            for (int lightIndex = 0; lightIndex < world.Lights.Length; lightIndex++)
            {
                LightNode light = world.Lights[lightIndex];

                int sampleCount = light.GetSampleCount();
                for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
                {
                    float3 L = light.CalculateRadiance(ref world, hitPoint.Point, hitPoint.Normal, sampleIndex, out float3 pointToLightDir);

                    float3 reflect = math.reflect(-pointToLightDir, hitPoint.Normal);

                    color += L * (diffuseColor * math.max(0.0f, math.dot(hitPoint.Normal, pointToLightDir))
                                  + SpecularColor * math.pow(math.max(0.0f, math.dot(reflect, pointToEyeDir.xyz)), Shininess));
                }
            }

            return color;
        }

        public float3 GetDiffuseColor(float2 texCoords)
        {
            return DiffuseColor * DiffuseTexture.SampleColor(texCoords);
        }

        public float3 GetBRDF(float3 pointToLight, float3 normal, float3 pointToEye)
        {
            return 1.0f / PI;
        }

        public void GetHemisphereSample(ref RNG random, out float3 hemDirection, out float sampleProbability)
        {
            float xi1 = random.float01;
            float xi2 = random.float01;

            hemDirection.x = cos(2 * PI * xi2) * sqrt(xi1);
            hemDirection.y = sin(2 * PI * xi2) * sqrt(xi1);
            hemDirection.z = sqrt(1 - xi1);

            sampleProbability = hemDirection.z / PI;
        }

        public void Dispose()
        {
            DiffuseTexture.Dispose();
        }

    }
}
