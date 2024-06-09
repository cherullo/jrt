using System.Linq;

using JRT.Data;
using JRT.World.Node;
using JRT.World.Light;

using UnityEngine;
using Unity.Collections;
using System;
using Unity.Mathematics;

namespace JRT.World
{
    public class WorldBuilder : MonoBehaviour
    {
        [SerializeField]
        [ColorUsage(false, true)]
        private Color _ambientLight;

        [SerializeField]
        private float _ambientPower;

        private NativeArray<GeometryNode> _geometryNodes;
        private NativeArray<LightNode> _lightNodes;

        public Data.World BuildWorld()
        {
            Data.World ret = new Data.World();
            ret.AmbientLight = (_ambientLight * _ambientPower).ToFloat3();

            if ((_lightNodes.IsCreated == true) || (_geometryNodes.IsCreated == true))
                OnDestroy();

            _GenerateNodes();

            ret.Geometries = _geometryNodes;
            ret.Lights = _lightNodes;

            return ret;
        }

        private void _GenerateNodes()
        {
            var lightComponents = FindObjectsOfType<BaseLightNode>(false).ToList();
            var geomComponents = FindObjectsOfType<BaseGeometryNode>(false);

            var lightNodesList = lightComponents.Select((node, index) =>
            {
                LightNode light = node.GetNodeData();
                light.Index = index;
                return light;
            });

            if (_ambientPower > 0)
            {
                lightNodesList = lightNodesList.Append(new LightNode()
                {
                    Index = lightNodesList.Count(),
                    Color = _ambientLight.ToFloat3(),
                    LocalToWorld = float4x4.identity,
                    Power = _ambientPower,
                    Type = Data.LightType.AmbientLight                    
                });
            }

            var lightNodes = lightNodesList.ToArray();

            _CalculateNormalizedAccumulatedPower(lightNodes);

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

        private void _CalculateNormalizedAccumulatedPower(LightNode[] lightNodes)
        {
            float totalPower = lightNodes.Sum(n => n.Power);
            if (totalPower <= 0.0f)
                throw new Exception("Total light power in scene is zero.");

            float acc = 0;
            for(int i = 0; i < lightNodes.Length - 1; i++)
            {
                float normalizedPower = lightNodes[i].Power / totalPower;
                acc += normalizedPower;
                lightNodes[i].NormalizedAccumulatedPower = acc;
            }
            lightNodes[^1].NormalizedAccumulatedPower = 1.0f;

            Debug.Log("Normalized Accumulated Power: " + string.Join(", ", lightNodes.Select(n => n.NormalizedAccumulatedPower.ToString())));
        }

        private void OnDestroy()
        {
            _geometryNodes.Dispose();
            _lightNodes.Dispose();
        }
    }
}
