using System;
using Veldrid;

namespace Flat2.Core.Renderer
{
    public class RenderContext : IDisposable
    {
        public GraphicsDevice Device { get; }
        public ResourceFactory Factory => Device.ResourceFactory;
        public Framebuffer SwapChainFrameBuffer => Device.MainSwapchain.Framebuffer;
        public CommandList CommandList { get; }
        public Texture MainColorTexture { get; private set; }
        public Framebuffer OffscreenFramebuffer { get; private set; }
        public Sampler LinearSampler { get; }
        public DeviceBuffer CameraBuffer { get; }
        public RenderContext(GraphicsDevice device)
        {
            Device = device;
            CommandList = Factory.CreateCommandList();
            LinearSampler = Device.LinearSampler;
            CameraBuffer = Factory.CreateBuffer(new BufferDescription(64 * 4, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            CommandList.Begin();
            CommandList.SetFramebuffer(SwapChainFrameBuffer);
            CommandList.ClearColorTarget(0, RgbaFloat.CornflowerBlue);
            CommandList.End();
            // 初始化主离屏画布，默认与屏幕尺寸一致
            CreateOffscreenResources(SwapChainFrameBuffer.Width, SwapChainFrameBuffer.Height);
        }
        private void CreateOffscreenResources(uint width, uint height)
        {
            MainColorTexture?.Dispose();
            OffscreenFramebuffer?.Dispose();
            MainColorTexture = Factory.CreateTexture(TextureDescription.Texture2D(
                width, height, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.RenderTarget | TextureUsage.Sampled));
            OffscreenFramebuffer = Factory.CreateFramebuffer(new FramebufferDescription(null, MainColorTexture));
        }

        public void Begin()
        {
            CommandList.Begin();
            CommandList.SetFramebuffer(OffscreenFramebuffer);
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
            CreateOffscreenResources(width, height);
        }

        public void Dispose()
        {
            MainColorTexture?.Dispose();
            OffscreenFramebuffer?.Dispose();
            CameraBuffer?.Dispose();
            CommandList?.Dispose();
        }
    }
}