using JRT.Data;
using UnityEngine;

namespace JRT
{
    [RequireComponent(typeof(Camera))]
    public class FilmAdapter : MonoBehaviour
    {
        private Camera _camera;

        private Texture2D _texture;

        public int Width { get; private set; }
        public int Height { get; private set; }

        private bool _applyPending = false;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            Width = Screen.width;
            Height = Screen.height;

            _texture = new Texture2D(Width, Height, TextureFormat.RGBAFloat, false, true);
            _texture.Apply();

            _camera.cullingMask = 0;
        }

        private void OnDestroy()
        {
            Destroy(_texture);
            _texture = null;
        }

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

            ret.Width = Width;
            ret.Height = Height;
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