using Flat2.Core.Comps;
using Flat2.Core.Renderer;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flat2.Core.Nodes
{
    public abstract class Node
    {
        public Node? Parent { get; private set; }
        public List<Node>? Children { get; private set; }
        public List<Comp>? Comps { get; private set; }
        protected Matrix4x4 _localTransform { get; private set; }
        protected Matrix4x4 _globalTransform { get; private set; }
        protected bool _dirty { get; private set; } = true;
        public int Layer { get; set; } = 0;
        public Matrix4x4 GlobalTransform
        {
            get
            {
                if (_dirty)
                {
                    _globalTransform = Parent == null ? _localTransform : _localTransform * Parent.GlobalTransform;
                    _dirty = false;
                }
                return _globalTransform;
            }
        }
        public abstract void OnLoad();
        public abstract void OnUpdate(double deltaTime);
                public abstract void OnRender(double deltaTime,RenderContext renderContext);
    }
}
