using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Nodes
{
    public class Scene:Node
    {
        public List<Node> Nodes { get; private set; }
        public Dictionary<int,ILayer> Layers { get; private set; }

    }
}
