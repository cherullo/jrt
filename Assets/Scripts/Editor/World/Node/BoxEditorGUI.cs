using JRT.World.Node;

using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

namespace JRT.Editor.World.Node
{
    [CustomEditor(typeof(Box))]
    public class BoxEditorGUI : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                Box box = target as Box;

                UnityEngine.Renderer renderer = target.GetComponent<UnityEngine.Renderer>();
                if (renderer != null)
                {
                    var color = box.DiffuseColor;
                    renderer.material.color = color;
                }
            }
        }
    }
}
