using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace JRT.Data
{
    public struct GeometryNode
    {
        public int Index;
        public GeometryType Type;
        public int LightIndex;

        public AABB Bounds;
        public float4x4 WorldToLocal;
        public float4x4 LocalToWorld;

        public Material Material;

        [ReadOnly]
        public UnsafeList<Triangle> Triangles;

        [ReadOnly]
        public UnsafeList<AABBTreeNode> Nodes;

        public bool IsValid()
        {
            return Type != GeometryType.Undefined;
        }

        public bool IsLightGeometry()
        {
            return IsValid() && (LightIndex != -1);
        }

        public bool IsIntersectedBy(Ray ray, out HitPoint hitPoint)
        {
            bool result;
            Ray localRay = ray.TransformToLocal(this);

            switch (Type)
            {
                default:
                case GeometryType.Undefined:
                    hitPoint = HitPoint.Invalid;
                    result = false;
                    break;

                case GeometryType.Box:
                    result = new AABB(-0.5f, 0.5f).IsIntersectedBy(localRay, out hitPoint);
                    break;

                case GeometryType.Sphere:
                    result = _IntersectOriginCenteredSphere(0.5f, localRay, out hitPoint);
                    break;

                case GeometryType.Mesh:
                    //result = _IntersectMesh(localRay, out hitPoint);
                    result = _IntersectMeshFast(localRay, out hitPoint);
                    break;
            }

            if (result == true)
                hitPoint = hitPoint.TransformToWorld(this);

            return result;
        }

        private bool _IntersectMeshFast(Ray ray, out HitPoint resultingHitPoint)
        {
            resultingHitPoint = HitPoint.Invalid;
            float t = float.MaxValue;

            UnsafeList<int> _nodeStack = new UnsafeList<int>(32, Allocator.TempJob);
            _nodeStack.Add(0);

            while (_nodeStack.IsEmpty == false)
            {
                int last = _nodeStack.Length - 1;
                int nodeIndex = _nodeStack[last];
                _nodeStack.RemoveAt(last);

                AABBTreeNode node = Nodes[nodeIndex];

                for (int i = 0; i < node.NumChildren; i++)
                {
                    HitPoint tempHP;
                    if (new AABB(node.MinAABBs[i], node.MaxAABBs[i]).IsIntersectedBy(ray, out tempHP) == false)
                        continue;
                    
                    if (tempHP.T > t)
                        continue;

                    if (node.ChildIsLeaf[i] == true)
                    {
                        int triangleIndex = node.ChildIndexes[i];
                        if (Triangles[triangleIndex].IsIntersectedBy(ray, out tempHP) == true)
                        {
                            if ((tempHP.FrontHit) && (tempHP.T < t))
                            {
                                t = tempHP.T;
                                resultingHitPoint = tempHP;
                            }
                        }
                    }
                    else
                    {
                        _nodeStack.Add(node.ChildIndexes[i]);
                    }
                }
            }

            _nodeStack.Dispose();
            return (t != float.MaxValue);
        }

        private bool _IntersectMesh(Ray ray, out HitPoint resultingHitPoint)
        {
            resultingHitPoint = HitPoint.Invalid;
            float t = float.MaxValue;

            for (int i = 0; i < Triangles.Length; i++)
            {
                if (Triangles[i].IsIntersectedBy(ray, out HitPoint hitPoint) == true)
                {
                    if ((hitPoint.FrontHit) && (hitPoint.T < t))
                    {
                        t = hitPoint.T;
                        resultingHitPoint = hitPoint;
                    }
                }
            }

            return (t != float.MaxValue);
        }

        [BurstCompile]
        private bool _IntersectOriginCenteredSphere(float radius, Ray ray, out HitPoint hitPoint)
        {
            float a = math.lengthsq(ray.Direction.xyz);
            float b = 2.0f * math.dot(ray.Direction.xyz, ray.Start.xyz);
            float c = math.lengthsq(ray.Start.xyz) - radius * radius;

            float delta = b * b - 4.0f * a * c;

            if (delta < 0)
            {
                hitPoint = HitPoint.Invalid;
                return false;
            }

            delta = math.sqrt(delta);
            float x1 = (-b - delta) / (2.0f * a);
            float x2 = (-b + delta) / (2.0f * a);

            if (x1 < 0.0f)
            {
                if (x2 < 0.0f)
                {
                    hitPoint = HitPoint.Invalid;
                    return false;
                }
                else
                {
                    hitPoint.T = x2;
                    hitPoint.FrontHit = false;
                }
            }
            else
            {
                hitPoint.T = x1;
                hitPoint.FrontHit = true;
            }

            hitPoint.Point = ray.Start + ray.Direction * hitPoint.T;
            hitPoint.Normal = 2.0f * hitPoint.Point.xyz; // The same as normalization since point rests in a origin centered sphere of radius 0.5f
            hitPoint.TexCoords.x = 0.5f + math.atan2(hitPoint.Normal.z, hitPoint.Normal.x) / (2.0f * math.PI);
            hitPoint.TexCoords.y = 0.5f + math.asin(hitPoint.Normal.y) / math.PI;
            return true;
        }

        public static GeometryNode Invalid {
            get
            {
                var ret = new GeometryNode();
                ret.LightIndex = -1;
                return ret;
            }
        }
    }
}
