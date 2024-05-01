using Unity.Mathematics;

namespace JRT.Data
{
    public struct AABBTreeNode
    {
        public int NumChildren;
        public float3x4 MaxAABBs;
        public float3x4 MinAABBs;
        public int4 ChildIndexes;
        public bool4 ChildIsLeaf;
    }
}
