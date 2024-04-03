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
        public NativeArray<UnityEngine.Color32> OutputColors;

        [BurstCompile]
        public void Execute()
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                OutputColors[i] = CalculatePixelColor(Pixels[i]);
            }
        }

        UnityEngine.Color32 CalculatePixelColor(int2 pixel)
        {
            Ray ray = Film.GenerateRay(pixel);

            float3 color = World.TraceRay(ray);

            int3 intColor = (int3) math.round(math.clamp(color, 0.0f, 1.0f) * 255);

            return new UnityEngine.Color32(
                (byte) intColor.x,
                (byte) intColor.y,
                (byte) intColor.z,
                255);
        }
    }
}
