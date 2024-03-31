using Unity.Collections;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct World 
    {
        [ReadOnly]
        public NativeArray<GeometryNode> Nodes;

        // TODO: Return geometryNode.Index
        public bool TraceRay(Ray ray, out GeometryNode hitNode, out HitPoint hitPoint)
        {
            float lastDistance = float.MaxValue;
            hitNode = GeometryNode.Invalid;
            hitPoint = HitPoint.Invalid;

            for (int i = 0; i < Nodes.Length; i++)
            {
                GeometryNode node = Nodes[i];

                if (node.Bounds.IsIntersectedBy(ray, out _) == false)
                    continue;

                if (node.IsIntersectedBy(ray, out HitPoint tempHitPoint) == false)
                    continue;

                float distance = math.lengthsq(ray.Start - tempHitPoint.Point);
                if (distance < lastDistance)
                {
                    lastDistance = distance;
                    hitNode = node;
                    hitPoint = tempHitPoint;
                }
            }

            return hitPoint.IsValid();
        }
    }
}
