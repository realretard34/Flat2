using System;
using System.Collections.Generic;
using System.Text;
using Veldrid;

namespace Flat2.Core.Renderer
{
    public abstract class RenderPass:IDisposable
    {
        public abstract void Dispose();
        public abstract void OnLoad(RenderContext renderContext);
        public abstract void OnRender(RenderContext renderContext);
        public abstract void OnResize(RenderContext renderContext);
    }
}
