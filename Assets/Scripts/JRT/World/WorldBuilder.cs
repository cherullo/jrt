using System.Linq;

using JRT.Data;
using JRT.World.Node;

using UnityEngine;
using Unity.Collections;

namespace JRT.World
{
    public class WorldBuilder : MonoBehaviour
    {
        private NativeArray<GeometryNode> _geometryNodes;

        public Data.World BuildWorld()
        {
            Data.World ret = new Data.World();

            if (_geometryNodes.IsCreated == false)
            {
                var nodeArray = FindObjectsOfType<BaseGeometryNode>(false).Select(x => x.GetNodeData()).ToArray();

                for (int i = 0; i < nodeArray.Length; i++)
                    nodeArray[i].Index = i;

                _geometryNodes = new NativeArray<GeometryNode>(nodeArray, Allocator.Persistent);
            }

            ret.Nodes = _geometryNodes;

            return ret;
        }

        private void OnDestroy()
        {
            if (_geometryNodes.IsCreated == true)
            {
                _geometryNodes.Dispose();
            }
        }
    }
}
