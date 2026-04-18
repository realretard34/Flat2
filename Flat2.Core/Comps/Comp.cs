using Flat2.Core.Nodes;
using Flat2.Core.Renderer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Comps
{
    public abstract class Comp
    {
        public abstract void OnLoad();
        public abstract void RequestRender(Scene scene, RenderContext renderContext);
        public abstract void OnUpdate(double deltaTime);
        public abstract void OnRender(double deltaTime, RenderContext renderContext);
    }
}
