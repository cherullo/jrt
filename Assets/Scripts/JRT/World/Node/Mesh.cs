using System;

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

using JRT.Data;
using JRT.Utils;
using System.Collections.Generic;
using Unity.Collections;

namespace JRT.World.Node
{
    public class Mesh : BaseGeometryNode
    {
        private UnsafeList<Triangle> _triangles;
        private UnsafeList<Data.AABBTreeNode> _nodes;

        public override GeometryType GetGeometryType()
        {
            return GeometryType.Mesh;
        }

        public override GeometryNode GetNodeData()
        {
            GeometryNode ret = GetBaseData();

            UnityEngine.Mesh mesh = _GetMesh();
            ret.Bounds = new AABB(mesh.bounds).Transform(ret.LocalToWorld);
            ret.Triangles = _GetAllTriangles(mesh);
            ret.Nodes = _BuildTree(ret.Triangles);
            return ret;
        }

        private UnsafeList<Triangle> _GetAllTriangles(UnityEngine.Mesh mesh)
        {
            int[] triangleIndexes = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uvs = mesh.uv;
            int triangleCount = triangleIndexes.Length / 3;

            Triangle[] triangles = new Triangle[triangleCount];

            for (int i = 0; i < triangleCount; i++)
            {
                Triangle tri = new Triangle();

                tri.P0 = vertices[triangleIndexes[i * 3 + 0]];
                tri.N0 = normals[triangleIndexes[i * 3 + 0]];
                tri.Tex0 = uvs[triangleIndexes[i * 3 + 0]];

                tri.P1 = vertices[triangleIndexes[i * 3 + 1]];
                tri.N1 = normals[triangleIndexes[i * 3 + 1]];
                tri.Tex1 = uvs[triangleIndexes[i * 3 + 1]];

                tri.P2 = vertices[triangleIndexes[i * 3 + 2]];
                tri.N2 = normals[triangleIndexes[i * 3 + 2]];
                tri.Tex2 = uvs[triangleIndexes[i * 3 + 2]];

                triangles[i] = tri;
            }

            _triangles = triangles.ToUnsafeList();
            return _triangles;
        }

        private UnityEngine.Mesh _GetMesh()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf != null)
                return mf.sharedMesh;
            
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                UnityEngine.Mesh bakedMesh = new UnityEngine.Mesh();
                smr.BakeMesh(bakedMesh, true);
                return bakedMesh;
            }

            throw new Exception($"No mesh in {gameObject.name}");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _triangles.Dispose();
            _nodes.Dispose();
        }

        private UnsafeList<Data.AABBTreeNode> _BuildTree(UnsafeList<Triangle> triangles)
        {
            List<Leaf> leafs = new List<Leaf>(triangles.Length);

            for (int i = 0; i < triangles.Length; i++)
                leafs.Add(new Leaf(i, triangles[i].CalculateAABB()));

            AABBTreeNode root = CreateAABBTree(leafs);

            return FlattenTree(root);
        }

        private UnsafeList<Data.AABBTreeNode> FlattenTree(AABBTreeNode root)
        {
            int nodeCount = _CountNodes(root);

            _nodes = new UnsafeList<Data.AABBTreeNode>(nodeCount, AllocatorManager.Persistent);
            _nodes.Resize(nodeCount);

            int nextFreeIndex = 1;
            _nodes[0] = FlattenNode(ref _nodes, root, ref nextFreeIndex);

            return _nodes;
        }

        private int _CountNodes(AABBTreeNode node)
        {
            if (node.IsLeaf == true)
                return 0;

            int count = 1;
            foreach (AABBTreeNode child in node.Children)
                count += _CountNodes(child);

            return count;
        }

        private Data.AABBTreeNode FlattenNode(ref UnsafeList<Data.AABBTreeNode> flatTree, AABBTreeNode node, ref int nextFreeIndex)
        {
            Data.AABBTreeNode newNode = new Data.AABBTreeNode();

            if (node.IsLeaf == true) // This should never happen
                throw new Exception("Leaf node should not be inserted directly in the flat tree.");

            newNode.NumChildren = node.Children.Count;
            int properChildren = 0;
            for (int i = 0; i < node.Children.Count; i++)
            {
                var child = node.Children[i];
                newNode.MaxAABBs[i] = child.AABB.Max;
                newNode.MinAABBs[i] = child.AABB.Min;
                newNode.ChildIsLeaf[i] = child.IsLeaf;

                if (child.IsLeaf == true)
                {
                    newNode.ChildIndexes[i] = child.LeafIndex;
                }
                else
                {
                    properChildren++;
                }
            }

            int savedIndex = nextFreeIndex;
            nextFreeIndex += properChildren;
            for (int i = 0; i < node.Children.Count; i++)
            {
                if (node.Children[i].IsLeaf == true)
                    continue;

                newNode.ChildIndexes[i] = savedIndex;
                flatTree[savedIndex++] = FlattenNode(ref flatTree, node.Children[i], ref nextFreeIndex);
            }

            return newNode;
        }

        private AABBTreeNode CreateAABBTree(List<Leaf> leafs)
        {
            if ((leafs == null) || (leafs.Count == 0))
                return null;

            AABB aabb = leafs[0].AABB;

            for(int i = 1; i < leafs.Count; i++)
                aabb.Encapsulate(leafs[i].AABB);

            AABBTreeNode ret = new AABBTreeNode();
            ret.AABB = aabb;

            // Sort
            IComparer<Leaf> comparer = _ChooseComparer(aabb);
            leafs.Sort(comparer);

            // Partition and recurse
            foreach (List<Leaf> chunk in leafs.Split(4))
            {
                if (chunk.Count == 0) 
                    continue;

                if (chunk.Count == 1)
                {
                    var leaf = chunk[0];
                    var child = new AABBTreeNode();
                    child.LeafIndex = leaf.Index;
                    child.AABB = leaf.AABB;
                    ret.Children.Add(child);
                }
                else
                {
                    ret.Children.Add(CreateAABBTree(chunk));
                }
            }

            return ret;
        }

        private IComparer<Leaf> _ChooseComparer(AABB aabb)
        {
            var delta = aabb.Max - aabb.Min;

            if ((delta.x >= delta.y) && (delta.x >= delta.z))
                return MaxXComparer.Default;

            if ((delta.y >= delta.x) && (delta.y >= delta.z))
                return MaxYComparer.Default;

            //if ((delta.z >= delta.x) && (delta.z >= delta.y))
                return MaxZComparer.Default;
        }

        private class MaxXComparer : IComparer<Leaf>
        {
            public int Compare(Leaf x, Leaf y)
            {
                return Comparer<float>.Default.Compare(x.AABB.Max.x, y.AABB.Max.x);
            }

            public static MaxXComparer Default = new MaxXComparer();
        }

        private class MaxYComparer : IComparer<Leaf>
        {
            public int Compare(Leaf x, Leaf y)
            {
                return Comparer<float>.Default.Compare(x.AABB.Max.y, y.AABB.Max.y);
            }

            public static MaxYComparer Default = new MaxYComparer();
        }

        private class MaxZComparer : IComparer<Leaf>
        {
            public int Compare(Leaf x, Leaf y)
            {
                return Comparer<float>.Default.Compare(x.AABB.Max.z, y.AABB.Max.z);
            }

            public static MaxZComparer Default = new MaxZComparer();
        }

        private class Leaf
        {
            public int Index;
            public AABB AABB;

            public Leaf(int index, AABB aabb)
            {
                Index = index;
                AABB = aabb;
            }
        }

        private class AABBTreeNode
        {
            public int LeafIndex = -1;
            public AABB AABB;
            public List<AABBTreeNode> Children = new List<AABBTreeNode>();

            public bool IsLeaf => LeafIndex >= 0;
        }
    }
}
