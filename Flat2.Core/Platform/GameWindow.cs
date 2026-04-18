using Flat2.Core.Nodes;
using Flat2.Core.Render;
using Flat2.Core.Renderer;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using System.Numerics;
using Veldrid;
using Veldrid.SPIRV;
namespace Flat2.Core.Platform
{
    public class GameWindow : IDisposable
    {
        public List<Scene> scenes = [];
        public RenderContext RenderContext { get; private set; }
        public Scene ActiveScene { get; private set; }
        public RenderFPL ActiveFPL { get; private set; }
        public readonly IWindow _window;
        public Vector2 Size { get => new(_window.Size.X, _window.Size.Y); set => _window.Size = new((int)value.X, (int)value.Y); }
        public InputMgr InputMgr { get; set; }
        public string Title { get => _window.Title; set => _window.Title = value; }
        public GameWindow(Vector2 size, string title = "Window1")
        {
            var options = WindowOptions.Default;
            options.Size = new((int)size.X, (int)size.Y);
            options.Title = title;
            options.API = GraphicsAPI.None;
            _window = Window.Create(options);
            _window.Load += OnWindowLoad;
            _window.Update += OnWindowUpdate;
            _window.Render += OnWindowRender;
            _window.FramebufferResize += OnFramebufferResize;
            InputMgr = new InputMgr(ref _window);
        }
        private void OnWindowRender(double deltaTime)
        {
         //  if (ActiveScene == null || ActiveFPL == null) return;
            ActiveFPL.OnRender(RenderContext, ActiveScene, deltaTime);
            //RenderContext.Begin();
            //RenderContext.CommandList.SetFramebuffer(RenderContext.SwapChainFrameBuffer);
            //RenderContext.CommandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

            //// 绘制三角形
            //RenderContext.CommandList.SetPipeline(pipeline);
            //RenderContext.CommandList.SetVertexBuffer(0, vertexBuffer);
            //RenderContext.CommandList.Draw(3);
            //RenderContext.Submit();
        }
        public void ChangeScene(Scene newScene)
        {
            //包含则切换，不包含则添加后切换
            if (ActiveScene != null && ActiveScene != newScene)
            {
            }
            if (!scenes.Contains(newScene)) scenes.Add(newScene);
            ActiveScene = newScene;
        }
        public void ChangeFPLTemp(RenderFPL fpl)
        {
            ActiveFPL = fpl;
        }
        public void ChangeSceneTemp(Scene scene)
        {
            ActiveScene = scene;
        }
        private void OnWindowUpdate(double deltaTime)
        {
            if (ActiveScene == null) return;
            ActiveScene.OnUpdate(deltaTime);
        }
        private void OnWindowLoad()
        {
            RenderContext = new RenderContext(_window.CreateGraphicsDevice());
//            Vertex[] vertices = {
//        new (new Vector2(0, 0.5f), RgbaFloat.Red),
//        new (new Vector2(0.5f, -0.5f), RgbaFloat.Green),
//        new (new Vector2(-0.5f, -0.5f), RgbaFloat.Blue)
//    };
//            vertexBuffer = RenderContext.Factory.CreateBuffer(new BufferDescription((uint)(vertices.Length * 24), BufferUsage.VertexBuffer));
//            RenderContext.Device.UpdateBuffer(vertexBuffer, 0, vertices);
//            string vertexShaderSource = @"
//#version 450
//layout(location = 0) in vec2 pos;
//layout(location = 1) in vec4 col;
//layout(location = 0) out vec4 fcol;
//void main() {
//    gl_Position = vec4(pos, 0, 1);
//    fcol = col;
//}";

//            string fragmentShaderSource = @"
//#version 450
//layout(location = 0) in vec4 fcol;
//layout(location = 0) out vec4 ocol;
//void main() {
//    ocol = fcol;
//}";

//            var shaders = RenderContext.Factory.CreateFromSpirv(
//                new ShaderDescription(
//                    ShaderStages.Vertex,
//                    System.Text.Encoding.UTF8.GetBytes(vertexShaderSource),
//                    "main"
//                ),
//                new ShaderDescription(
//                    ShaderStages.Fragment,
//                    System.Text.Encoding.UTF8.GetBytes(fragmentShaderSource),
//                    "main"
//                )
//            );

//            // --- 配置管线 ---
//            pipeline = RenderContext.Factory.CreateGraphicsPipeline(new GraphicsPipelineDescription
//            {
//                BlendState = BlendStateDescription.SingleOverrideBlend,
//                DepthStencilState = DepthStencilStateDescription.Disabled,
//                RasterizerState = RasterizerStateDescription.Default,
//                PrimitiveTopology = PrimitiveTopology.TriangleList,
//                ShaderSet = new ShaderSetDescription(
//                    new[] { new VertexLayoutDescription(
//                new VertexElementDescription("pos", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
//                new VertexElementDescription("col", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate)) },
//                    shaders),
//                ResourceLayouts = Array.Empty<ResourceLayout>(),
//                Outputs = RenderContext.Device.MainSwapchain.Framebuffer.OutputDescription
//            });
            //相信InputMgr
        }
        private void OnFramebufferResize(Vector2D<int> newSize)
        {
               RenderContext.Resize((uint)newSize.X, (uint)newSize.Y);
        }

        public void Run()
        {
            _window.Run();
        }
        public void Dispose()
        {
            Console.WriteLine(_window.Title + " Dispose");
            _window.Close();
            _window?.Dispose();
        }
    }
}
