using JRT.Data;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace JRT.World.Node
{
    public class Mesh : BaseGeometryNode
    {
        private UnsafeList<Triangle> _triangles;

        public override GeometryType GetGeometryType()
        {
            return GeometryType.Mesh;
        }

        public override GeometryNode GetNodeData()
        {
            GeometryNode ret = GetBaseData();

            UnityEngine.Mesh mesh = _GetMesh();
            ret.Bounds = new AABB(mesh.bounds).Transform(ret.LocalToWorld);

            int[] triangleIndexes = mesh.triangles;
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uvs = mesh.uv;
            int triangleCount = triangleIndexes.Length / 3;

            _triangles = new UnsafeList<Triangle>(triangleCount, AllocatorManager.Persistent);
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

                _triangles.AddNoResize(tri);
            }

            ret.Triangles = _triangles;

            return ret;
        }

        private UnityEngine.Mesh _GetMesh()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf != null)
                return mf.sharedMesh;
            
            SkinnedMeshRenderer smr = GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
                return smr.sharedMesh;

            throw new Exception($"No mesh in {gameObject.name}");
        }

        private void OnDestroy()
        {
            if (_triangles.IsCreated)
                _triangles.Dispose();
        }
    }
}
