using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Nodes
{
    public class Scene : BaseNode
    {
        public override void OnDestroy()
        {
            Console.WriteLine("Scene destroyed");
        }

        public override void OnLoad()
        {
            Console.WriteLine("Scene loaded");
        }

        public override void OnRender()
        {
            Console.WriteLine("Scene rendered");
        }

        public override void OnUpdate(double deltaTime)
        {
            Console.WriteLine("Scene updated");
        }
    }
}
