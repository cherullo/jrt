using Unity.Mathematics;

namespace JRT.Data
{
    public struct LightNode 
    {
        public int Index;
        public LightType Type;

        public float4 Color;
        public float4 Position;

        public bool IsValid()
        {
            return Type != LightType.Undefined;
        }

        public static LightNode Invalid => new LightNode();
    }
}
