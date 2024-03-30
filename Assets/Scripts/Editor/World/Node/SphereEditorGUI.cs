using JRT.World.Node;

using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

namespace JRT.Editor.World.Node
{
    [CustomEditor(typeof(Sphere))]
    public class SphereEditorGUI : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                Sphere box = target as Sphere;

                UnityEngine.Renderer renderer = target.GetComponent<UnityEngine.Renderer>();
                if (renderer != null)
                {
                    var color = box.Color;
                    renderer.material.color = new Color(color.x, color.y, color.z);
                }
            }
        }
    }
}
