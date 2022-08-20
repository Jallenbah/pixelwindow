var rand = new Random();

var window = new PixelWindow.PixelWindow(1024, 576, 8, "Big pixels",
    (renderWindow) => {
        // On load function - runs once at start.
        // The SFML render window provides ability to set up events and input (maybe store a reference to it for later use in your update functions)
    },
    (frameTime) => {
        // Update function - process update logic to run every frame here
    },
    (deltaTime) => {
        // Fixed update function - process logic to run every fixed timestep here
    },
    (pixelData, frameTime) => {
        // Render function - set pixel data for the current frame here
        // Randomised pixels shown as example.
        pixelData.Clear();
        for (uint x = 0; x < pixelData.Width; x++)
        {
            for (uint y = 0; y < pixelData.Height; y++)
            {
                pixelData[x, y] = ((byte)rand.Next(0, 255), (byte)rand.Next(0, 255), (byte)rand.Next(0, 255));
            }
        }
    }, fixedTimestep: 20, framerateLimit: 300);

window.Run();