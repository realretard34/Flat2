using Flat2.Core.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Nodes
{
    public class Scene : BaseNode
    {
        public string SceneName { get; init; }
        public Camera? MainCamera { get; set; }
        public override void OnDestroy()
        {
            Console.WriteLine("Scene destroyed");
        }
        public static bool operator ==(Scene? a, Scene? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.SceneName == b.SceneName;
        }

        public static bool operator !=(Scene? a, Scene? b) => !(a == b);

        public override bool Equals(object? obj) => obj is Scene other && this == other;

        public override int GetHashCode() => SceneName?.GetHashCode() ?? 0;

        public override void OnLoad()
        {
            Console.WriteLine("Scene loaded");
        }

        public override void OnRender(double deltaTime,RenderContext ctx)
        {
            if (MainCamera == null) { 
                foreach (var child in Children)
                {
                    if(child is Camera camera)
                    {
                        MainCamera = camera;
                        break;
                    }
                }
            }
            Console.WriteLine("Scene rendered");
            base.OnRender(deltaTime, ctx);
        }

        public override void OnUpdate(double deltaTime)
        {
            Console.WriteLine("Scene updated");
            base.OnUpdate(deltaTime);
        }
    }
}
