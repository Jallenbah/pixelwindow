namespace PixelWindow
{
    /// <summary>
    /// Class representing RGB pixel data for reading and writing
    /// </summary>
    internal class PixelData
    {
        /// <summary>
        /// The width of the pixel grid
        /// </summary>
        public uint Width { get; private set; }

        /// <summary>
        /// The height of the pixel grid
        /// </summary>
        public uint Height { get; private set; }

        /// <summary>
        /// Raw RGBA data. The alpha component is not used, except for being set to 255 for full opacity.
        /// This object is used for rendering with SFML by updating render texture data.
        /// </summary>
        public byte[] RawData { get; private set; }

        /// <summary>
        /// Creates a new pixel data instance. Only the <see cref="PixelWindow"/> should need to do this.
        /// </summary>
        /// <param name="width">The width of the pixel grid</param>
        /// <param name="height">The height of the pixel grid</param>
        public PixelData(uint width, uint height)
        {
            Width = width;
            Height = height;
            RawData = new byte[4 * width * height]; // 4 bytes per pixel (R, G, B, A)
        }

        // For a given X and Y coordinate, gets the index of the 1 dimensional array.
        private uint GetIndexFromXY(uint x, uint y) => 4 * ((y * Width) + x);

        /// <summary>
        /// Gets or sets the pixel data at the specified coordinates
        /// </summary>
        /// <param name="x">The column of the pixel</param>
        /// <param name="y">The row of the pixel</param>
        /// <returns>Pixel data as a tuple of 3 bytes - one for each R, G, and B component</returns>
        public (byte r, byte g, byte b) this[uint x, uint y]
        {
            get
            {
                var i = GetIndexFromXY(x, y);
                return (RawData[i], RawData[i + 1], RawData[i + 2]);
            }
            set
            {
                var i = GetIndexFromXY(x, y);
                RawData[i] = value.r;
                RawData[i + 1] = value.g;
                RawData[i + 2] = value.b;
                RawData[i + 3] = 255; // We don't care about opacity, set alpha to opaque
            }
        }

        /// <summary>
        /// Clears all bytes in the pixel data to 0
        /// </summary>
        public void Clear()
        {
            Array.Clear(RawData);
        }
    }
}
