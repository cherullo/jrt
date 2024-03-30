
using Unity.Mathematics;

namespace JRT.Data
{
    public struct Film 
    {
        public float4x4 CameraLocalToWorld;

        public int Width;
        public int Height;

        public float NearPlane;
        public float FieldOfView;
        public float AspectRatio;

        public Ray GenerateRay(int2 pixel)
        {
            float filmHalfHeight = NearPlane * math.tan(math.radians(0.5f * FieldOfView));
            float filmHalfWidth = filmHalfHeight * AspectRatio;

            float pixelHeight = 2.0f * filmHalfHeight / Height;
            float pixelWidth = 2.0f * filmHalfWidth / Width;

            float3 direction3 = new float3(
                -filmHalfWidth + (pixel.x + 0.5f) * pixelWidth, 
                -filmHalfHeight + (pixel.y + 0.5f) * pixelHeight,
                NearPlane);

            direction3 = math.normalize(direction3);

            return new Ray(CameraPosition, math.mul(CameraLocalToWorld, new float4(direction3, 0.0f)));
        }

        public float4 CameraPosition => CameraLocalToWorld.c3;

        public float4 CameraRight => CameraLocalToWorld.c0;

        public float4 CameraUp => CameraLocalToWorld.c1;

        public float4 CameraForward => CameraLocalToWorld.c2;
    }
}
