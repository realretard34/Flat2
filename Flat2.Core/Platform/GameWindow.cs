using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Input;
using System.Drawing;
using Flat2.Core.Nodes;
using Flat2.Core.Render;
namespace Flat2.Core.Platform
{
    public class GameWindow:IDisposable 
    {
        public List<Scene> scenes = [];
        public RenderContext _renderContext { get; private set; }
        public Scene ActiveScene { get; private set; }
        public readonly IWindow _window;
        public Vector2 Size { get { return new Vector2(_window.Size.X, _window.Size.Y); } set { _window.Size = new((int)value.X, (int)value.Y); } }
        public InputMgr _inputMgr { get; set; }
        public string Title { get { return _window.Title; } set { _window.Title = value; } }
        public GameWindow(Vector2 size, string title="Window1") {
            var options = WindowOptions.Default;
            options.Size = new((int)size.X, (int)size.Y);
            options.Title = title;
            _window = Window.Create(options);
            _window.Load += OnWindowLoad;
            _window.Update += OnWindowUpdate;
            _window.Render += OnWindowRender;
            _window.FramebufferResize += OnFramebufferResize;
            _inputMgr = new InputMgr(ref _window);
        }
        private void OnWindowRender(double deltaTime)
        {
             if (ActiveScene == null) return;
            //获取当前场景的主相机，设置渲染上下文的投影矩阵

            _renderContext.BeginFrame();
            ActiveScene.OnRender(deltaTime, _renderContext);
            _renderContext.EndFrame();
        }
        public void ChangeScene(Scene newScene)
        {
            //包含则切换，不包含则添加后切换
            if (ActiveScene != null && ActiveScene != newScene)
            {
                ActiveScene.Dispose();
            }
            if (!scenes.Contains(newScene)) scenes.Add(newScene);
            ActiveScene = newScene;
        }
        public void ChangeSceneTemp(Scene scene) {
            //临时切换，不添加到场景列表中
            ActiveScene = scene;
        }
        private void OnWindowUpdate(double deltaTime)
        {
            if (ActiveScene == null) return;
            ActiveScene.OnUpdate(deltaTime);
        }
        private void OnWindowLoad()
        {
            _renderContext = new RenderContext(_window);
            //相信InputMgr
        }
        private void OnFramebufferResize(Vector2D<int> newSize)
        {
            _renderContext?.ResizeViewport(newSize.X, newSize.Y);
            _window.DoRender();
            Console.WriteLine(Size);
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
