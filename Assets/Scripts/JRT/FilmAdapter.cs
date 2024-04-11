using System;

using JRT.Data;
using JRT.Sampling;

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

        private Color32[] _colors;

        public ISampler Sampler { get; private set; }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _camera.cullingMask = 0;

            Width = Mathf.CeilToInt(_resolutionScale * Screen.width);
            Height = Mathf.CeilToInt(_resolutionScale * Screen.height);

            _texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false, false);
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.alphaIsTransparency = false;
            _texture.anisoLevel = 0;
            _texture.filterMode = FilterMode.Point;
            _texture.Apply();

            _colors = new Color32[Width * Height];

            Sampler = GetComponent<ISampler>() ?? DefaultSampler.Instance;
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
                _texture.SetPixelData(_colors, 0);
                _texture.Apply(false);
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
            ret.Sampler = Sampler.GetSamplerData();

            return ret;
        }

        public void SetPixel(int x, int y, Color32 outputColor)
        {
            int baseIndex = (x + y * Width);
            _colors[baseIndex] = outputColor;

            _applyPending = true;
        }
    }
}