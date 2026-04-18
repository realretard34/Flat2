using Flat2.Core.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flat2.Core.Nodes
{
    public interface ILayer
    {
        int Index { get; }
        Matrix4x4 GetFinalMatrix(Node node, Camera camera, RenderContext context);
    }
}
