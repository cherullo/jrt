using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;

using JRT.Data;
using JRT.World;

using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace JRT.Renderer
{
    public class RayTracer : MonoBehaviour
    {
        public int blockWidth = 8;
        public int blockHeight = 8;

        [SerializeField]
        private FilmAdapter _film;
        
        [SerializeField]
        private WorldBuilder _worldBuilder;

        private List<(RenderBlockJob, JobHandle)> _jobs;
        private Stopwatch _stopwatch = new Stopwatch();

        void Start()
        {
            StartRender(_film, _worldBuilder);
        }

        private void StartRender(FilmAdapter filmAdapter, WorldBuilder worldBuilder)
        {
            Data.World world = worldBuilder.BuildWorld();
            Data.Film film = filmAdapter.GetFilmData();

            _jobs = _ScheduleJobs(blockWidth, blockHeight, film, world);
            JobHandle.ScheduleBatchedJobs();

            Debug.Log("Starting render");
            Debug.Log($"Resolution: {film.Width}x{film.Height}");
            Debug.Log($"Nodes: {world.Nodes.Length}");
            Debug.Log($"Block size: {blockWidth}x{blockHeight}");
            Debug.Log($"Scheduled {_jobs.Count} jobs.");
            _stopwatch.Reset();
            _stopwatch.Start();
        }

        private List<(RenderBlockJob, JobHandle)> _ScheduleJobs(int blockWidth, int blockHeight, Film film, Data.World world)
        {
            List<(RenderBlockJob, JobHandle)> ret = new List<(RenderBlockJob, JobHandle)>();

            for (int y = 0; y < film.Height; y += blockHeight)
            {
                int actualBlockHeight = math.min(blockHeight, film.Height - y);

                for (int x = 0; x < film.Width; x += blockWidth)
                {
                    int actualBlockWidth = math.min(blockWidth, film.Width - x);

                    int blockPixelCount = actualBlockWidth * actualBlockHeight;

                    RenderBlockJob job = new RenderBlockJob();
                    job.World = world;
                    job.Film = film;
                    job.Pixels = _GeneratePixels(x, y, actualBlockWidth, actualBlockHeight);
                    job.OutputColors = new NativeArray<float3>(blockPixelCount, Allocator.Persistent);

                    ret.Add((job, job.Schedule()));
                }
            }

            return ret;
        }

        private NativeArray<int2> _GeneratePixels(int x, int y, int blockWidth, int blockHeight)
        {
            int2[] ret = new int2 [blockWidth * blockHeight];

            int i = 0;
            for (int px = 0; px < blockWidth; px++)
            {
                for (int py = 0; py < blockHeight; py++)
                {
                    ret[i++] = new int2(x + px, y + py);
                }
            }

            return new NativeArray<int2>(ret, Allocator.Persistent);
        }

        void Update()
        {
            if ((_jobs == null) || (_jobs.Count == 0))
                return;

            for (int jobIndex = 0; jobIndex < _jobs.Count; jobIndex++)
            {
                (RenderBlockJob job, JobHandle handle) = _jobs[jobIndex];

                if (handle.IsCompleted == false)
                    continue;

                handle.Complete();

                for (int pixelIndex = 0; pixelIndex < job.Pixels.Length; pixelIndex++)
                {
                    int2 pixel = job.Pixels[pixelIndex];
                    float3 outputColor = job.OutputColors[pixelIndex];
                    _film.SetPixel(pixel.x, pixel.y, new Color(outputColor.x, outputColor.y, outputColor.z));
                }

                job.OutputColors.Dispose();
                job.Pixels.Dispose();

                int lastIndex = _jobs.Count - 1;
                _jobs[jobIndex] = _jobs[lastIndex];
                _jobs.RemoveAt(lastIndex);
                jobIndex--;
            }

            if (_jobs.Count == 0)
            {
                _stopwatch.Stop();
                Debug.Log($"Rendering finished after {_stopwatch.Elapsed.TotalSeconds}s");
            }
        }
    }
}
