using System;
using Veldrid;
using Veldrid.SPIRV; // 确保引用了 Veldrid.SPIRV 库

namespace Flat2.Core.Renderer
{
    public class BlitPass : RenderPass
    {
        private Pipeline _pipeline;
        private ResourceLayout _layout;
        private ResourceSet _resourceSet;

        public override void OnLoad(RenderContext ctx)
        {
            var factory = ctx.Factory;

            // 1. 定义资源布局
            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SourceTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SourceSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

            // 2. 直接在 Pass 内部定义/加载 Shader
            Shader[] shaders = factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, System.Text.Encoding.UTF8.GetBytes(VertexCode), "main"),
                new ShaderDescription(ShaderStages.Fragment, System.Text.Encoding.UTF8.GetBytes(FragmentCode), "main"));

            // 3. 创建管线
            var pd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                DepthStencilStateDescription.Disabled,
                RasterizerStateDescription.CullNone,
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(Array.Empty<VertexLayoutDescription>(), shaders),
                new[] { _layout },
                ctx.SwapChainFrameBuffer.OutputDescription);

            _pipeline = factory.CreateGraphicsPipeline(ref pd);

            // 4. 初始化资源绑定
            UpdateTexture(ctx);
        }
        public override void OnResize(RenderContext renderContext)
        {
            UpdateTexture(renderContext);
        }
        public void UpdateTexture(RenderContext ctx)
        {
            _resourceSet?.Dispose();
            _resourceSet = ctx.Factory.CreateResourceSet(new ResourceSetDescription(
                _layout, ctx.MainColorTexture, ctx.LinearSampler));
        }

        public override void OnRender(RenderContext ctx)
        {
            var cl = ctx.CommandList;

            // 强制将渲染目标切换到屏幕，把 OSFB 内容画上去
            cl.SetFramebuffer(ctx.SwapChainFrameBuffer);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _resourceSet);
            cl.Draw(3);
        }

        public override void Dispose()
        {
            _pipeline?.Dispose();
            _layout?.Dispose();
            _resourceSet?.Dispose();
        }

        // --- Shader 字符串定义 ---
        private const string VertexCode = @"
#version 450
layout(location = 0) out vec2 fsin_uv;
void main() {
    fsin_uv = vec2((gl_VertexIndex << 1) & 2, gl_VertexIndex & 2);
    gl_Position = vec4(fsin_uv * 2.0f - 1.0f, 0.0f, 1.0f);
    gl_Position.y = -gl_Position.y; 
}";

        private const string FragmentCode = @"
#version 450
layout(set = 0, binding = 0) uniform texture2D t_Source;
layout(set = 0, binding = 1) uniform sampler s_Sampler;
layout(location = 0) in vec2 fsin_uv;
layout(location = 0) out vec4 fsout_Color;
void main() {
    fsout_Color = texture(sampler2D(t_Source, s_Sampler), fsin_uv);
}";
    }
}