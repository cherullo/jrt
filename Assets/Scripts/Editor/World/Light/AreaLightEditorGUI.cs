using Unity.Mathematics;

using UnityEditor;
using UnityEngine;

namespace JRT.World.Light
{
    [CustomEditor(typeof(AreaLight))]
    public class AreaLightEditorGUI : UnityEditor.Editor
    {
        void OnSceneGUI()
        {
            AreaLight light = target as AreaLight;
            if (light == null)
                return;
            
            Vector3 forward = 0.25f * light.transform.forward;
            float3[] samplingPoints = light.GenerateSamplingPoints();

            for (int i = 0; i < samplingPoints.Length; i++)
            {
                Vector3 point = samplingPoints[i];
                Handles.DrawLine(point, point + forward);
            }
        }
    }
}
