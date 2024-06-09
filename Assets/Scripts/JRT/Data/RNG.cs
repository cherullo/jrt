using Unity.Mathematics;

namespace JRT.Data
{
    // https://www.pcg-random.org/
    // https://www.shadertoy.com/view/XlGcRh
    // https://youtu.be/Qz0KTGYJtUk?si=hWBglwcRsdcOqi0c&t=655
    public struct RNG 
    {
        public uint State;

        public float float01
        {
            get
            {
                {
                    State = State * 747796405u + 2891336453u;
                    uint result = ((State >> ((int)(State >> 28) + 4)) ^ State) * 277803737u;
                    result = (result >> 22) ^ result;
                    return (float)(result / 4294967295.0);
                }
            }
        }

        public float3 Color
        {
            get
            {
                return new float3(float01, float01, float01);
            }
        }

        public float2 UnitSquare
        {
            get
            {
                return new float2(float01, float01);
            }
        }

        public float3 UnitSphere
        {
            get
            {
                float u = float01;
                float v = float01;

                float phi = math.acos(1 - 2 * u);
                float theta = v * 2 * math.PI;

                return new float3(
                    math.cos(theta) * math.sin(phi),
                    math.cos(phi),
                    math.sin(theta) * math.sin(phi)
                    );
            }
        }

        public int Index(int sampleCount)
        {
            return (int) math.trunc(sampleCount * float01 * 0.99999f);
        }
    }
}
