using JRT.Data;
using System;
using UnityEditor;
using UnityEngine;

namespace JRT
{
    [RequireComponent(typeof(Camera))]
    public class FilmAdapter : MonoBehaviour
    {
        [SerializeField]
        private float _resolutionScale = 1.0f;
        private Camera _camera;

        private Texture2D _texture;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private bool _applyPending = false;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            Width = Mathf.CeilToInt(_resolutionScale * Screen.width);
            Height = Mathf.CeilToInt(_resolutionScale * Screen.height);

            _texture = new Texture2D(Width, Height, TextureFormat.RGBAFloat, false, true);
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.Apply();

            _camera.cullingMask = 0;
        }

        private void OnDestroy()
        {
            Destroy(_texture);
            _texture = null;
        }

#if UNITY_EDITOR
        [ContextMenu("Save Image")]
        public void SaveImage()
        {
            string dateString = DateTime.Now.ToString("dd_MM_yyyy-H_mm");
            string defaultName = "Screenshot-" + dateString + ".png";

            string filename = EditorUtility.SaveFilePanel("Save Screenshot", "~", defaultName, "png");
            byte[] data = _texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(filename, data);
        }
#endif

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (_applyPending == true)
            {
                _applyPending = false;
                _texture.Apply();
            }

            Graphics.Blit(_texture, destination);
        }

        public Film GetFilmData()
        {
            Film ret = new Film();

            ret.Width = _texture.width;
            ret.Height = _texture.height;
            ret.NearPlane = _camera.nearClipPlane;
            ret.FieldOfView = _camera.fieldOfView;
            ret.AspectRatio = _camera.aspect;

            ret.CameraLocalToWorld = transform.localToWorldMatrix;

            return ret;
        }

        public void SetPixel(int x, int y, Color color)
        {
            _texture.SetPixel(x, y, color);
            _applyPending = true;
        }
    }
}