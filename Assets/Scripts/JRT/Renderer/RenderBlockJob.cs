using JRT.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace JRT.Renderer
{
    [BurstCompile]
    public struct RenderBlockJob : IJob
    {
        public RenderType Type;
        public Data.World World;
        public Data.Film Film;

        [ReadOnly]
        public NativeArray<int2> Pixels;

        [WriteOnly]
        public NativeArray<UnityEngine.Color32> OutputColors;

        [BurstCompile]
        public void Execute()
        {
            Film.Initialize();

            for (int i = 0; i < Pixels.Length; i++)
            {
                switch (Type)
                {
                    default:
                    case RenderType.RayTracing:
                        OutputColors[i] = CalculatePixelColor(Pixels[i]);
                        break;
                    case RenderType.PathTracing:
                        OutputColors[i] = TracePixelPath(Pixels[i]);
                        break;
                }
            }
        }

        UnityEngine.Color32 TracePixelPath(int2 pixel)
        {
            return new UnityEngine.Color32();
        }

        UnityEngine.Color32 CalculatePixelColor(int2 pixel)
        {
            int sampleCount = Film.Sampler.SampleCount;

            float3 color = 0;
            for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
            {
                Ray ray = Film.GenerateRay(ref World.Random, pixel, sampleIndex);

                color += World.TraceRay(ray);
            }

            int3 intColor = (int3)math.round(math.clamp(color / sampleCount, 0.0f, 1.0f) * 255);

            return new UnityEngine.Color32(
                (byte)intColor.x,
                (byte)intColor.y,
                (byte)intColor.z,
                255);
        }
    }
}
