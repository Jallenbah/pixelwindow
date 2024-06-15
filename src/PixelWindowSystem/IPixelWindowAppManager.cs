namespace PixelWindowSystem
{
    /// <summary>
    /// Interface to implement for controlling the <see cref="PixelWindow"/>.
    /// An instance of a class implementing this interface must be passed into the constructor of <see cref="PixelWindow"/>.
    /// </summary>
    public interface IPixelWindowAppManager
    {
        /// <summary>
        /// On load function. This is run once at the start of the app running and can be used for
        /// setting up input events, loading data etc.
        /// </summary>
        /// <param name="renderWindow">The <see cref="SFML.Graphics.RenderWindow"/> used by the <see cref="PixelWindow"/></param>
        void OnLoad(SFML.Graphics.RenderWindow renderWindow);

        /// <summary>
        /// Update function, for updating non-render data every frame (e.g. handling input)
        /// </summary>
        /// <param name="frameTime">The time in ms since the last frame</param>
        void Update(float frameTime);

        /// <summary>
        /// Fixed update function, for changing non-render data at a fixed rate independent of rendering (e.g. 50hz)
        /// </summary>
        /// <param name="timeStep">
        /// The fixed timestep configured on the <see cref="PixelWindow"/> in ms. This can and
        /// should be used as the deltatime value for any physics calculations for example.
        /// </param>
        void FixedUpdate(float timeStep);

        /// <summary>
        /// Render function, for updating the raw pixel data
        /// </summary>
        /// <param name="pixelData">
        /// The <see cref="PixelData"/> object which represents the raw RGB values of the pixels on screen. Changes to this are
        /// rendered this frame.
        /// </param>
        /// <param name="frameTime">The time in ms since the last frame</param>
        /// <remarks>
        /// Make sure to call <see cref="PixelData.Clear"/> at the start of the render function if you want to clear
        /// the screen before you start rendering!
        /// </remarks>
        void Render(PixelData pixelData, float frameTime);
    }
}
