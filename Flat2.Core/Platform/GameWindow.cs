using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Drawing;
using Flat2.Core.Nodes;
namespace Flat2.Core.Platform
{
    public class GameWindow:IDisposable
    {
        public List<Scene> scenes = [];
        public readonly IWindow _window;
        public Vector2 size { get; set { _window.Size = new((int)value.X, (int)value.Y); } }
        public InputMgr _inputMgr { get; set; }
        public string title { get { return _window.Title; } set { _window.Title = value; } }
        public GameWindow(Vector2 size, string title="Window1") {
            var options = WindowOptions.Default;
            options.Size = new((int)size.X, (int)size.Y);
            options.Title = title;
            _window = Window.Create(options);
            //Assign events.
            _window.Load += OnWindowLoad;
            _window.Update += OnWindowUpdate;
            _window.Render += OnWindowRender;
            _window.FramebufferResize += OnFramebufferResize;
            _inputMgr = new InputMgr(ref _window);
        }
        private void OnWindowRender(double deltaTime)
        {
            //Here all rendering should be done.
        }

        private void OnWindowUpdate(double deltaTime)
        {
              foreach(var scene in scenes)
                if (scene.isActive)
                     scene.OnUpdate(deltaTime);
        }

        private void OnWindowLoad()
        {
            //相信InputMgr
        }
        private void OnFramebufferResize(Vector2D<int> newSize)
        {
            size=new Vector2(newSize.X, newSize.Y);
            Console.WriteLine(size);
            //Update aspect ratios, clipping regions, viewports, etc.
        }
        public void Run()
        {
             _window.Run();
        }
        public void Dispose()
        {
            Console.WriteLine(_window.Title+" Dispose");
            _window.Close();
            _window?.Dispose();
        }
    }
}
