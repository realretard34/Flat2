using Flat2.Core.Renderer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Nodes
{
    public class Scene:Node
    {
        public Camera? MainCamera { get; private set; }
        public List<Node> Nodes { get; private set; } = [];
        public Dictionary<int, ILayer> Layers { get; private set; } = [];
        public void AddLayer(ILayer layer)
        {
            Layers[layer.Index] = layer;
        }
        public override void OnLoad()
        {
            throw new NotImplementedException();
        }

        public override void OnRender(double deltaTime, RenderContext renderContext)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].OnRender(deltaTime, renderContext);
                for (int j = 0; j < (Nodes[i].Comps?.Count ?? 0); j++)
                {
                    Nodes[i]?.Comps?[j]?.OnRender(deltaTime, renderContext);
                }
            }
        }

        public override void OnUpdate(double deltaTime)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Nodes[i].OnUpdate(deltaTime);
                for (int j = 0; j < (Nodes[i].Comps?.Count ?? 0); j++)
                {
                    Nodes[i]?.Comps?[j]?.OnUpdate(deltaTime);
                }
            }
        }
    }
}
