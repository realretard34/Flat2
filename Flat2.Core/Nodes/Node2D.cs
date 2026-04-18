using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flat2.Core.Nodes
{
    public abstract class Node2D: Node
    {
        public Vector2 Position { get; set; }
        public Vector2 Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public int ZIndex { get; set; } = -1;
    }
}
