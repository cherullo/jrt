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
                color += R * world.TraceRay(new Ray(hitPoint.Point, new float4(reflect, 0.0f)));

            return color;
        }

        public float3 CalculatePhongColor(ref World world, Ray ray, HitPoint hitPoint)
        {
            float3 diffuseColor = DiffuseColor * DiffuseTexture.SampleColor(hitPoint.TexCoords);
            float3 color = diffuseColor * world.AmbientLight;
            float4 pointToEyeDir = new float4(math.normalize((ray.Start - hitPoint.Point).xyz), 0.0f);

            for (int lightIndex = 0; lightIndex < world.Lights.Length; lightIndex++)
            {
                LightNode light = world.Lights[lightIndex];

                int sampleCount = light.GetSampleCount();
                for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
                {
                    float3 L = light.CalculateRadiance(ref world, hitPoint.Point, sampleIndex, out float3 pointToLightDir);

                    float3 reflect = math.reflect(-pointToLightDir, hitPoint.Normal);

                    color += L * (diffuseColor * math.max(0.0f, math.dot(hitPoint.Normal, pointToLightDir))
                                  + SpecularColor * math.pow(math.max(0.0f, math.dot(reflect, pointToEyeDir.xyz)), Shininess));
                }
            }

            return color;
        }

        public void Dispose()
        {
            DiffuseTexture.Dispose();
        }
    }
}
