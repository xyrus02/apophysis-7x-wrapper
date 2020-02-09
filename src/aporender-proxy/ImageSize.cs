namespace Apophysis
{
    public struct ImageSize
    {
        public ImageSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
        
        public int Width { get; }
        public int Height { get; }
    }
}