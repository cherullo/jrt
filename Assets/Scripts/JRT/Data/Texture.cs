using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct Texture 
    {
        public int Width; 
        public int Height;
        public UnsafeList<float3> Pixels;

        public bool IsValid()
        {
            return !(Width == 0 || Height == 0);
        }

        public float3 SampleColor(float2 texCoords)
        {
            if (IsValid() == false)
                return 1.0f;

            texCoords = math.clamp(texCoords, 0.0f, 0.99999f);
            texCoords = math.trunc(texCoords * new float2(Width, Height));
            
            return Pixels[(int) (texCoords.x + texCoords.y * Width)];
        }

        public static Texture Invalid
        {
            get => new Texture();
        }
    }
}
