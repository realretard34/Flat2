using Silk.NET.Windowing;
using Silk.NET.Windowing.Extensions.Veldrid;
using Veldrid;
using Veldrid.SPIRV; // 记得 NuGet 安装这个包
using System.Numerics;
using System.Text.Json;
using System.Xml;

var options = WindowOptions.Default with {API=GraphicsAPI.None};

var window = Window.Create(options);
GraphicsDevice? graphicsDevice = null;
ResourceFactory? factory = null;
CommandList? commandList = null;
Pipeline? pipeline = null;
DeviceBuffer? vertexBuffer = null;

window.Load += () =>
{
    // 创建 Veldrid 设备 (使用你的参数)
    graphicsDevice = window.CreateGraphicsDevice(new GraphicsDeviceOptions
    {
        PreferStandardClipSpaceYDirection = true, // 建议用 Inverted 保持坐标系习惯
        PreferDepthRangeZeroToOne = true,

    });
    factory = graphicsDevice.ResourceFactory;
    commandList = factory.CreateCommandList();
    Vertex[] vertices = {
        new (new Vector2(0, 0.5f), RgbaFloat.Red),
        new (new Vector2(0.5f, -0.5f), RgbaFloat.Green),
        new (new Vector2(-0.5f, -0.5f), RgbaFloat.Blue)
    };
    vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(vertices.Length * 24), BufferUsage.VertexBuffer));
    graphicsDevice.UpdateBuffer(vertexBuffer, 0, vertices);
    string vertexShaderSource = @"
#version 450
layout(location = 0) in vec2 pos;
layout(location = 1) in vec4 col;
layout(location = 0) out vec4 fcol;
void main() {
    gl_Position = vec4(pos, 0, 1);
    fcol = col;
}";

    string fragmentShaderSource = @"
#version 450
layout(location = 0) in vec4 fcol;
layout(location = 0) out vec4 ocol;
void main() {
    ocol = fcol;
}";

    // 2. 分别转换为字节数组并创建 Shader
    var shaders = factory.CreateFromSpirv(
        new ShaderDescription(
            ShaderStages.Vertex,
            System.Text.Encoding.UTF8.GetBytes(vertexShaderSource),
            "main"
        ),
        new ShaderDescription(
            ShaderStages.Fragment,
            System.Text.Encoding.UTF8.GetBytes(fragmentShaderSource),
            "main"
        )
    );

    // --- 配置管线 ---
    pipeline = factory.CreateGraphicsPipeline(new GraphicsPipelineDescription
    {
        BlendState = BlendStateDescription.SingleOverrideBlend,
        DepthStencilState = DepthStencilStateDescription.Disabled,
        RasterizerState = RasterizerStateDescription.Default,
        PrimitiveTopology = PrimitiveTopology.TriangleList,
        ShaderSet = new ShaderSetDescription(
            new[] { new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("col", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate)) },
            shaders),
        ResourceLayouts = Array.Empty<ResourceLayout>(),
        Outputs = graphicsDevice.MainSwapchain.Framebuffer.OutputDescription
    });
};

window.Render += (delta) =>
{
    if (commandList == null || graphicsDevice == null || pipeline == null) return;
    Console.WriteLine($"经过{delta}来到了下一帧");
    commandList.Begin();
    commandList.SetFramebuffer(graphicsDevice.MainSwapchain.Framebuffer);
    commandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

    // 绘制三角形
    commandList.SetPipeline(pipeline);
    commandList.SetVertexBuffer(0, vertexBuffer);
    commandList.Draw(3);

    commandList.End();
    graphicsDevice.SubmitCommands(commandList);
    graphicsDevice.SwapBuffers();
};

window.FramebufferResize += (size) =>
{
    // 注意：在 Veldrid 中，Resize 的方法通常是 UpdateMainWindowSize
    graphicsDevice?.ResizeMainWindow((uint)size.X, (uint)size.Y);
};
window.Closing += () =>
{
    graphicsDevice?.Dispose();
    commandList?.Dispose();
    pipeline?.Dispose();
    vertexBuffer?.Dispose();
};

window.Run();
struct Vertex
{
    public Vector2 Position;
    public RgbaFloat Color;
    public Vertex(Vector2 pos, RgbaFloat col) { Position = pos; Color = col; }
}
