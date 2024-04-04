using Unity.Collections;
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

        public MultiSamplingType MultiSamplingType;
        [ReadOnly]
        public NativeArray<float2> SamplingPoints; // Deltas in the range (0, 1)

        private float2 filmHalfSize;
        private float2 pixelSize;

        public int GetSampleCount()
        {
            return SamplingPoints.Length;
        }

        public Ray GenerateRay(int2 pixel, int sampleIndex)
        {
            float2 sampleDelta = SamplingPoints[sampleIndex];

            // TODO: Vectorize
            //float filmHalfHeight = NearPlane * math.tan(math.radians(0.5f * FieldOfView));
            //float filmHalfWidth = filmHalfHeight * AspectRatio;

            //float pixelHeight = 2.0f * filmHalfHeight / Height;
            //float pixelWidth = 2.0f * filmHalfWidth / Width;

            //float3 direction3 = new float3(
            //    -filmHalfWidth + (pixel.x + sampleDelta.x) * pixelWidth, 
            //    -filmHalfHeight + (pixel.y + sampleDelta.y) * pixelHeight,
            //    NearPlane);

            float3 direction3 = new float3((pixel + sampleDelta) * pixelSize - filmHalfSize, NearPlane);

            direction3 = math.normalize(direction3);

            return new Ray(CameraPosition, math.mul(CameraLocalToWorld, new float4(direction3, 0.0f)));
        }

        public void Initialize()
        {
            filmHalfSize.y = NearPlane * math.tan(math.radians(0.5f * FieldOfView));
            filmHalfSize.x = filmHalfSize.y * AspectRatio;

            pixelSize.y = 2.0f * filmHalfSize.y / Height;
            pixelSize.x = 2.0f * filmHalfSize.x / Width;

            if (MultiSamplingType == MultiSamplingType.Random)
            {
                // TODO: Initialize random points
            }
        }

        public float4 CameraPosition => CameraLocalToWorld.c3;

        public float4 CameraRight => CameraLocalToWorld.c0;

        public float4 CameraUp => CameraLocalToWorld.c1;

        public float4 CameraForward => CameraLocalToWorld.c2;
    }
}
