using System;
using System.Runtime.InteropServices;
using System.Numerics;
using Veldrid;
using Veldrid.SPIRV;
using Flat2.Core.Nodes;
using System.Runtime.InteropServices;

namespace Flat2.Core.Renderer
{
    public class TriPass : RenderPass
    {
        private Pipeline _pipeline;
        private DeviceBuffer _vertexBuffer;
        private Shader[] _shaders;

        private readonly Vertex[] _vertices = new Vertex[] {
            new Vertex(new Vector2(0, 0.5f), RgbaFloat.Red),
            new Vertex(new Vector2(0.5f, -0.5f), RgbaFloat.Green),
            new Vertex(new Vector2(-0.5f, -0.5f), RgbaFloat.DarkRed)
        };

        public override void OnLoad(RenderContext renderContext)
        {
            // Create shaders
            string vertexShaderSource = @"#version 450
layout(location = 0) in vec2 pos;
layout(location = 1) in vec4 col;
layout(location = 0) out vec4 fcol;
void main() {
    gl_Position = vec4(pos, 0, 1);
    fcol = col;
}";

            string fragmentShaderSource = @"#version 450
layout(location = 0) in vec4 fcol;
layout(location = 0) out vec4 ocol;
void main() {
    ocol = fcol;
}";

            _shaders = renderContext.Factory.CreateFromSpirv(
                new ShaderDescription(ShaderStages.Vertex, System.Text.Encoding.UTF8.GetBytes(vertexShaderSource), "main"),
                new ShaderDescription(ShaderStages.Fragment, System.Text.Encoding.UTF8.GetBytes(fragmentShaderSource), "main")
            );

            // Create vertex buffer
            uint vertexSize = 24; // vec2 (2 floats) + vec4 (4 floats) = 6 floats * 4 = 24 bytes
            _vertexBuffer = renderContext.Factory.CreateBuffer(new BufferDescription((uint)_vertices.Length * vertexSize, BufferUsage.VertexBuffer));
            renderContext.Device.UpdateBuffer(_vertexBuffer, 0, _vertices);

            // Create pipeline
            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("pos", VertexElementFormat.Float2, VertexElementSemantic.TextureCoordinate),
                new VertexElementDescription("col", VertexElementFormat.Float4, VertexElementSemantic.TextureCoordinate)
            );

            var shaderSet = new ShaderSetDescription(new[] { vertexLayout }, _shaders);

            var pipelineDesc = new GraphicsPipelineDescription
            {
                BlendState = BlendStateDescription.SingleOverrideBlend,
                DepthStencilState = DepthStencilStateDescription.Disabled,
                RasterizerState = RasterizerStateDescription.Default,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = shaderSet,
                ResourceLayouts = Array.Empty<ResourceLayout>(),
                Outputs = renderContext.OffscreenFramebuffer.OutputDescription
            };

            _pipeline = renderContext.Factory.CreateGraphicsPipeline(pipelineDesc);
        }

        public override void OnRender(RenderContext renderContext)
        {
            var cl = renderContext.CommandList;
            cl.SetPipeline(_pipeline);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.Draw(3);
        }
        public override void Dispose()
        {
            _vertexBuffer?.Dispose();
            foreach (var s in _shaders ?? Array.Empty<Shader>())
                s?.Dispose();
            _pipeline?.Dispose();
        }

        public override void OnResize(RenderContext renderContext)
        {
            return;
        }
    }
}
