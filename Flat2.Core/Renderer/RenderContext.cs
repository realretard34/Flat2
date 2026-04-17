using Veldrid;

namespace Flat2.Core.Renderer
{
    public class RenderContext : IDisposable
    {
        public GraphicsDevice Device { get; }
        public ResourceFactory Factory => Device.ResourceFactory;
        public Framebuffer SwapChainFrameBuffer => Device.MainSwapchain.Framebuffer;
        public CommandList CommandList { get; }

        public Sampler LinearSampler { get; }
        public DeviceBuffer CameraBuffer { get; } // 存储相机矩阵的常量缓冲
        public RenderContext(GraphicsDevice device)
        {
            Device = device;
            CommandList = Factory.CreateCommandList();
            LinearSampler = Device.LinearSampler;
            CameraBuffer = Factory.CreateBuffer(new BufferDescription(128, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            }

        public void Begin()
        {
            CommandList.Begin();
        }

        public void Submit()
        {
            CommandList.End();
            Device.SubmitCommands(CommandList);
            Device.SwapBuffers();
        }
        public void Resize(uint width, uint height)
        {
            Device.ResizeMainWindow(width, height);
        }
        public void Dispose()
        {
            CommandList.Dispose();
        }
    }
}

