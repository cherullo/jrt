using System.Linq;

using JRT.Data;
using JRT.World.Node;
using JRT.World.Light;

using UnityEngine;
using Unity.Collections;

namespace JRT.World
{
    public class WorldBuilder : MonoBehaviour
    {
        [SerializeField]
        private Color _ambientLight;

        private NativeArray<GeometryNode> _geometryNodes;
        private NativeArray<LightNode> _lightNodes;

        public Data.World BuildWorld()
        {
            Data.World ret = new Data.World();
            ret.AmbientLight = _ambientLight.ToFloat3();

            if ((_lightNodes.IsCreated == true) || (_geometryNodes.IsCreated == true))
                OnDestroy();

            _GenerateNodes();

            ret.Geometries = _geometryNodes;
            ret.Lights = _lightNodes;

            return ret;
        }

        private void _GenerateNodes()
        {
            var lightComponents = FindObjectsOfType<BaseLightNode>(false).ToList();//.Select(x => x.GetNodeData()).ToArray();
            var geomComponents = FindObjectsOfType<BaseGeometryNode>(false);//.Select(x => x.GetNodeData()).ToArray();

            var lightNodes = lightComponents.Select((node, index) =>
            {
                LightNode light = node.GetNodeData();
                light.Index = index;
                return light;
            }).ToArray();

            var geomNodes = geomComponents.Select((comp, index) =>
            {
                GeometryNode geom = comp.GetNodeData();
                geom.Index = index;

                if (comp.Light != null)
                {
                    geom.LightIndex = lightComponents.IndexOf(comp.Light);
                }
                else
                {
                    geom.LightIndex = -1;
                }

                return geom;
            }).ToArray();

            _lightNodes = new NativeArray<LightNode>(lightNodes, Allocator.Persistent);
            _geometryNodes = new NativeArray<GeometryNode>(geomNodes, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            if (_geometryNodes.IsCreated == true)
                _geometryNodes.Dispose();

            if (_lightNodes.IsCreated == true)
                _lightNodes.Dispose();
        }
    }
}
