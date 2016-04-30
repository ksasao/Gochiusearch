using System;
namespace Mpga.ImageSearchEngine
{
    public class Argb32DataWithSize
    {
        public Argb32DataWithSize(int width, int height, byte[] payload)
        {
            Width = width;
            Height = height;
            Payload = payload;
        }

        public int Width { get; }

        public int Height { get; }

        public byte[] Payload { get; }
    }
}

