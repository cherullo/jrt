using Unity.Collections;

namespace JRT.Data
{
    public struct World 
    {
        [ReadOnly]
        public NativeArray<GeometryNode> Nodes;

        public bool TraceRay(Ray ray, out GeometryNode hitNode, out HitPoint hitPoint)
        {
            // float lastDistance = float.MaxValue;
            hitPoint = new HitPoint();

            for (int i = 0; i < Nodes.Length; i++)
            {
                GeometryNode node = Nodes[i];

                if (node.Bounds.IsIntersectedBy(ray) == false)
                    continue;
                
                hitNode = node;
                    
                return true;
            }

            hitNode = new GeometryNode();
            return false;
        }
    }
}
