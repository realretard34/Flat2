using Flat2.Core.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Nodes
{
    public class TestNode:BaseNode
    {
        public override void OnRender(double s,RenderContext ctx)
        {
            Console.WriteLine("rendering TestNode");
        }
    }
}
