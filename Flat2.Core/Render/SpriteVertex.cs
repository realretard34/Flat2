using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Flat2.Core.Render
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct SpriteVertex
    (
         Vector2 Position,//8
         Vector2 TexCoord,//8
         Vector4 Color,//16
          float Depth//4
    );
}
