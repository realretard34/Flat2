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
            //Assign events.
            _window.Load += OnWindowLoad;
            _window.Update += OnWindowUpdate;
            _window.Render += OnWindowRender;
            _window.FramebufferResize += OnFramebufferResize;
            _inputMgr = new InputMgr(ref _window);
        }
        private void OnWindowRender(double deltaTime)
        {
            if (ActiveScene == null) return;
            Camera? mainCamera = ActiveScene.MainCamera;
            if (mainCamera != null)
            {
                mainCamera.ViewportSize = Size;
                _renderContext.SetClearColor(
                    new Vector4(
                    mainCamera.BackgroundColor.X,
                    mainCamera.BackgroundColor.Y,
                    mainCamera.BackgroundColor.Z,
                    mainCamera.BackgroundColor.W
                    )
                );
                _renderContext.ProjectionView = mainCamera.GetProjectionViewMatrix();
            }
            _renderContext.BeginFrame();
            ActiveScene.OnRender(deltaTime, _renderContext);
            _renderContext.EndFrame();
        }
        public void ChangeScene(Scene scene){
            foreach  (var s in scenes){
                if(s == scene){
                    ActiveScene = s;
                    return;
                }
            }
            scenes.Add(scene);
            ActiveScene = scene;
        }
        public void ChangeSceneTemp(Scene scene) {
            ActiveScene = scene;
        }
        private void OnWindowUpdate(double deltaTime)
        {
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
