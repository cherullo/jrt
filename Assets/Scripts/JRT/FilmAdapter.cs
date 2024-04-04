using JRT.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEditor;
using UnityEngine;

namespace JRT
{
    [RequireComponent(typeof(Camera))]
    public class FilmAdapter : MonoBehaviour
    {
        [SerializeField]
        private float _resolutionScale = 1.0f;

        [SerializeField]
        private MultiSamplingType _multiSamplingType;

        [SerializeField]
        private int _sampleCount;

        private Camera _camera;

        private Texture2D _texture;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int SampleCount { get { return _sampleCount; } }

        private bool _applyPending = false;

        private Color32[] _colors;
        private float2[] _samplingPoints;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            Width = Mathf.CeilToInt(_resolutionScale * Screen.width);
            Height = Mathf.CeilToInt(_resolutionScale * Screen.height);

            _texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false, false);
            _texture.wrapMode = TextureWrapMode.Clamp;
            _texture.alphaIsTransparency = false;
            _texture.anisoLevel = 0;
            _texture.filterMode = FilterMode.Point;
            
            _texture.Apply();

            _colors = new Color32[Width * Height];

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
                _texture.SetPixelData(_colors, 0);
                _texture.Apply(false);
            }

            Graphics.Blit(_texture, destination);
        }

        public void Init()
        {
            _samplingPoints = null;
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

            ret.MultiSamplingType = _multiSamplingType;
            //ret.SampleCount = _sampleCount;
            // ret.SamplingPoints = _GetSamplingPoints();

            return ret;
        }

        public NativeArray<float2> GetSamplingPoints()
        {
            switch (_multiSamplingType)
            {
                case MultiSamplingType.SquarePattern:
                    if (_samplingPoints == null)
                        _samplingPoints = _GenerateSquarePattern(_sampleCount);
                    break;
                case MultiSamplingType.Halton:
                    if (_samplingPoints == null)
                        _samplingPoints = _GenerateHalton(_sampleCount);
                    break;
                default:
                case MultiSamplingType.Random:
                    _samplingPoints = _GenerateRandom(_sampleCount);
                    break;
            }

            return new NativeArray<float2>(_samplingPoints, Allocator.Persistent);
        }

        private float2[] _GenerateRandom(int sampleCount)
        {
            float2[] ret = new float2[_sampleCount];

            for (int i = 0; i < _sampleCount; i++)
                ret[i] = new float2(
                    UnityEngine.Random.Range(0.0f, 1.0f),
                    UnityEngine.Random.Range(0.0f, 1.0f)
                    );

            return ret;
        }

        private float2[] _GenerateHalton(int sampleCount)
        {
            float2[] ret = new float2[_sampleCount];

            IEnumerator<float> h2 = _HaltonSequence(2);
            IEnumerator<float> h3 = _HaltonSequence(3);

            for (int i = 0; i < sampleCount; i++)
            {
                h2.MoveNext();
                h3.MoveNext();
                ret[i] = new float2(h2.Current, h3.Current);
            }

            _PrintPattern(ret);

            return ret;
        }

        private float2[] _GenerateSquarePattern(int sampleCount)
        {
            float2[] ret = new float2[_sampleCount];
            int root = (int) Mathf.Sqrt(sampleCount);
            float pitch = 1.0f / (root + 1);

            for (int iy = 0; iy < root; iy++)
                for (int ix = 0; ix < root; ix++)
                {
                    float2 point = new float2((ix + 1) * pitch, (iy + 1) * pitch);
                    ret[ix + iy * root] = point;
                }

            _PrintPattern(ret);

            return ret;
        }

        private void _PrintPattern(float2[] points)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < points.Length; i++)
            {
                sb.Append(points[i].x);
                sb.Append("\t");
                sb.Append(points[i].y);
                sb.Append("\r\n");
            }

            Debug.Log(sb.ToString());
        }

        private float _Halton(int b, int index)
        {
            float result = 0.0f;
            float f = 1.0f;
            while (index > 0)
            {
                f = f / (float)b;
                result += f * (float)(index % b);
                index = index / b;
            }
            return result;
        }

        private IEnumerator<float> _HaltonSequence(int b) {
            float n = 0.0f;
            float d = 1.0f;
            while (true) {
                float x = d - n;
                if (x == 1) {
                    n = 1;
                    d *= b;
                } else {
                    float y = d / b;
                    while (x <= y)
                        y /= b;
                    n = (b + 1) * y - x;
                }
                yield return n / d;
            }
        }


        public void SetPixel(int x, int y, Color32 outputColor)
        {
            int baseIndex = (x + y * Width);
            _colors[baseIndex] = outputColor;

            _applyPending = true;
        }
    }

}