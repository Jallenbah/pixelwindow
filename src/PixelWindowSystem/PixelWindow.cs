using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Diagnostics;

namespace PixelWindowSystem
{
    /// <summary>
    /// A window class which allows high speed rendering of direct pixel data to the screen, at either 1:1 pixel size, or enlarged pixels
    /// </summary>
    public class PixelWindow
    {
        // Width and height of the window, and size of texture pixel in screen pixels (4 would be a 4x4 "pixel" - blown up 4x the size of screen pixels)
        private readonly uint _width, _height, _pixelScale;

        // Size of the actual pixel data being rendered
        private uint _renderWidth => _width / _pixelScale;
        private uint _renderHeight => _height / _pixelScale;

        // A model representing the raw pixel data that can be directly written to in the render function
        private PixelData _pixelData;

        // Window title
        private readonly string _title;

        // App manager, implementing our onload, update, fixed update, and render functions used within the Run method
        private readonly IPixelWindowAppManager _appManager;

        // The fixed timestep in ms used for the fixed update
        private readonly float _fixedTimestep;

        // SFML objects. A render texture is used to blow up pixels to a larger size. That render texture is drawn using a sprite
        // which has been scaled to the size of the window.
        private RenderWindow? _renderWindow { get; set; }
        private RenderTexture? _renderTexture { get; set; }
        private Sprite? _renderTextureSprite { get; set; }

        /// <summary>
        /// Sets up and opens a new window with the specified parameters
        /// </summary>
        /// <param name="width">The actual screen pixel width of the window</param>
        /// <param name="height">The actual screen pixel height of the window</param>
        /// <param name="pixelScale">The size of each pixel. 1 would be a 1:1 scale of screen pixels to drawn pixels. 2 would be double sized pixels etc.</param>
        /// <param name="title">The window title. Performance debug data will be periodically appended to this also.</param>
        /// <param name="onLoad">
        /// On load function, run once at the start, for setting up input events, loading data etc.
        /// Receives the SFML RenderWindow as a parameter (this has everything required to set up events and query input)
        /// </param>
        /// <param name="update">
        /// Update function, for updating non-render data every frame (e.g. handling input).
        /// Receives the frametime in ms as a parameter.
        /// </param>
        /// <param name="fixedUpdate">
        /// Update function, for changing non-render data at a fixed rate independent of rendering (e.g. 50hz).
        /// Receives the fixed timestep period in ms as a parameter.
        /// </param>
        /// <param name="render">
        /// Render function, for updating the pixel data every frame.
        /// Recevies the pixel data to be written to, and the frametime in ms as parameters.
        /// </param>
        /// <param name="framerateLimit">The actual screen pixel width of the window</param>
        public PixelWindow(uint width, uint height, uint pixelScale, string title, IPixelWindowAppManager appManager,
            float fixedTimestep = 20, uint framerateLimit = 300)
        {
            _width = width;
            _height = height;
            _pixelScale = pixelScale;

            _title = title ?? "Title";

            _appManager = appManager;

            _fixedTimestep = fixedTimestep;

            _pixelData = new PixelData(_renderWidth, _renderHeight);

            SetupSfmlWindow(framerateLimit);
        }

        private void SetupSfmlWindow(uint framerateLimit)
        {
            _renderWindow = new RenderWindow(
                new VideoMode(_width, _height),
                _title,
                Styles.Close);

            _renderWindow.SetFramerateLimit(framerateLimit);

            _renderTexture = new RenderTexture(_renderWidth, _renderHeight);
            _renderTexture.Texture.Smooth = false; // As we are blowing up the size of the pixels, we need to do this so it doesn't end up blurring it

            _renderTextureSprite = new Sprite(_renderTexture.Texture);
            _renderTextureSprite.Scale = new Vector2f(_pixelScale, _pixelScale);

            _renderWindow.Closed += new EventHandler((object? sender, EventArgs e) => {
                ((Window)sender!).Close();
            });

            _appManager.OnLoad(_renderWindow);
        }

        /// <summary>
        /// Runs the window loop. Should be called after creation.
        /// </summary>
        public void Run()
        {
            // Used for displaying debug performance info in the titlebar, along with stopwatch timings and iteration counts
            const int titleDebugInfoFrequencyMs = 500;

            double perf_totalUpdateMs = 0, perf_totalFixedUpdateMs = 0, perf_totalPreRenderMs = 0,
                   perf_totalRenderMs = 0, perf_totalPostRenderMs = 0;
            int perf_numberOfIterationsTimed = 0;
            int perf_numberOfFixedTimestepIterationsTimed = 0;

            var performanceStopwatch = new Stopwatch();
            var debugInfoUpdateStopwatch = new Stopwatch();
            debugInfoUpdateStopwatch.Start();

            double frameTimeAccumulatorMs = 0;
            var frameTimeStopwatch = new Stopwatch();
            frameTimeStopwatch.Start();

            // Main update loop
            while (_renderWindow!.IsOpen)
            {
                var frameTime = (float)frameTimeStopwatch.Elapsed.TotalMilliseconds;
                frameTimeAccumulatorMs += frameTime;
                frameTimeStopwatch.Restart();

                _renderWindow.DispatchEvents();

                RunProcessAndAddToTotalTime(() => { _appManager.Update(frameTime); }, ref perf_totalUpdateMs, performanceStopwatch);

                while (frameTimeAccumulatorMs >= _fixedTimestep)
                {
                    RunProcessAndAddToTotalTime( () => { _appManager.FixedUpdate(_fixedTimestep); }, ref perf_totalFixedUpdateMs, performanceStopwatch);
                    perf_numberOfFixedTimestepIterationsTimed++;
                    frameTimeAccumulatorMs -= _fixedTimestep;
                }

                RunProcessAndAddToTotalTime(Prerender, ref perf_totalPreRenderMs, performanceStopwatch);
                RunProcessAndAddToTotalTime(() => { _appManager.Render(_pixelData, frameTime); }, ref perf_totalRenderMs, performanceStopwatch);
                RunProcessAndAddToTotalTime(Postrender, ref perf_totalPostRenderMs, performanceStopwatch);

                perf_numberOfIterationsTimed++;

                // Update title bar with debug info
                if (debugInfoUpdateStopwatch.ElapsedMilliseconds >= titleDebugInfoFrequencyMs)
                {
                    double getAverageAndResetTime(ref double totalMs, int iterationCount)
                    {
                        var averageMs = totalMs / iterationCount;
                        totalMs = 0;
                        return averageMs;
                    };

                    var newTitle = $"{_title} - " +
                        $"Update: {       getAverageAndResetTime(ref perf_totalUpdateMs, perf_numberOfIterationsTimed)                   :0.0}ms | " +
                        $"Fixed Update: { getAverageAndResetTime(ref perf_totalFixedUpdateMs, perf_numberOfFixedTimestepIterationsTimed) :0.0}ms | " +
                        $"Prerender: {    getAverageAndResetTime(ref perf_totalPreRenderMs, perf_numberOfIterationsTimed)                :0.0}ms | " +
                        $"Render: {       getAverageAndResetTime(ref perf_totalRenderMs, perf_numberOfIterationsTimed)                   :0.0}ms | " +
                        $"Postrender: {   getAverageAndResetTime(ref perf_totalPostRenderMs, perf_numberOfIterationsTimed)               :0.0}ms";
                    _renderWindow.SetTitle(newTitle);

                    perf_numberOfIterationsTimed = 0;
                    perf_numberOfFixedTimestepIterationsTimed = 0;
                    debugInfoUpdateStopwatch.Restart();
                }
            }
        }

        private void RunProcessAndAddToTotalTime(Action? action, ref double totalTime, Stopwatch stopwatch)
        {
            stopwatch.Reset();
            stopwatch.Start();
            action!();
            stopwatch.Stop();
            totalTime += stopwatch.Elapsed.TotalMilliseconds;
        }

        private void Prerender()
        {
            _renderTexture!.Clear();
        }

        private void Postrender()
        {
            _renderTexture!.Texture.Update(_pixelData.RawData);
            _renderWindow!.Clear();
            _renderWindow.Draw(_renderTextureSprite);
            _renderWindow.Display();
        }
    }
}
