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
        public Data.World World;
        public Data.Film Film;

        [ReadOnly]
        public NativeArray<int2> Pixels;

        [WriteOnly]
        public NativeArray<float3> OutputColors;

        [BurstCompile]
        public void Execute()
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                OutputColors[i] = CalculatePixelColor(Pixels[i]);
            }
        }

        float3 CalculatePixelColor(int2 pixel)
        {
            Ray ray = Film.GenerateRay(pixel);

            return World.TraceRay(ray);
        }
    }
}
