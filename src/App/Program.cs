using PixelWindowSystem;
using SFML.Graphics;

var appManager = new RandomPixelsAppManager();

var window = new PixelWindow(1024, 576, 8, "Big pixels", appManager,
    fixedTimestep: 20, framerateLimit: 300);

window.Run();

// All we need to do is implement the methods in IPixelWindowAppManager and pass in an instance of this class to the
// PixelWindow constructor, as above.
class RandomPixelsAppManager : IPixelWindowAppManager
{
    private Random _rand = new();

    public void OnLoad(RenderWindow renderWindow)
    {
        // On load function - runs once at start.
        // The SFML render window provides ability to set up events and input (maybe store a reference to it for later use in your update functions)
    }

    public void Update(double frameTime)
    {
        // Update function - process update logic to run every frame here
    }

    public void FixedUpdate(double timeStep)
    {
        // Fixed update function - process logic to run every fixed timestep here
    }

    public void Render(PixelData pixelData, double frameTime)
    {
        // Render function - set pixel data for the current frame here
        // Randomised pixels shown as example.
        pixelData.Clear();
        for (uint x = 0; x < pixelData.Width; x++)
        {
            for (uint y = 0; y < pixelData.Height; y++)
            {
                pixelData[x, y] = ((byte)_rand.Next(0, 255), (byte)_rand.Next(0, 255), (byte)_rand.Next(0, 255));
            }
        }
    }
}