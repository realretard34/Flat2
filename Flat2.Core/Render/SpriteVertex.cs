using System.Numerics;
using System.Runtime.InteropServices;

namespace Flat2.Core.Render
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SpriteVertex
    {
        public Vector2 Position; // 8 bytes
        public Vector2 TexCoord; // 8 bytes
        public Vector4 Color;    // 16 bytes
        public float TexIndex;   // 4 bytes (用作多纹理插槽的索引)

        public SpriteVertex(Vector2 position, Vector2 texCoord, Vector4 color, float texIndex)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
            TexIndex = texIndex;
        }
    }
}