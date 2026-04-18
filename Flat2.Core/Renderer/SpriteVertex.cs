using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Flat2.Core.Renderer
{
    public struct Vertex
    {
        public Vector2 Position;
        public RgbaFloat Color;
        public Vertex(Vector2 pos, RgbaFloat col) { Position = pos; Color = col; }
    }
}