using UnityEngine;

namespace JRT
{
    [RequireComponent (typeof(Camera))]
    public class Film : MonoBehaviour
    {
        private Camera _camera;

        private Texture2D _texture;

        private int _width;
        private int _height;

        private void Awake()
        {
            _camera = GetComponent<Camera> ();

            _width = Screen.width;
            _height = Screen.height;

            _texture = new Texture2D(_width, _height, TextureFormat.RGBAFloat, false, true);
        }

        private void OnDestroy()
        {
            Destroy(_texture); 
            _texture = null;
        }

        public void SetPixel(int x, int y, Color color)
        {
            _texture.SetPixel (x, y, color);
        }

        public void ApplyTexture()
        {
            _texture.Apply();
        }
    }
}