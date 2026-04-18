using Flat2.Core.Nodes;
using Flat2.Core.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.SPIRV;

namespace Flat2.Core.Comps
{
    public class TriComp : Comp
    {
        private DeviceBuffer _vertexBuffer;
        private Pipeline _pipeline;
        private Shader[] _shaders;
        private Vertex[] _vertices = new Vertex[] {
            new Vertex(new Vector2(0,1f), Veldrid.RgbaFloat.Red),
            new Vertex(new Vector2(0.5f, -0.5f), Veldrid.RgbaFloat.CornflowerBlue),
            new Vertex(new Vector2(-0.5f, -0.5f), Veldrid.RgbaFloat.Blue)
        };

        public override void OnLoad()
        {
            // nothing here; resources created lazily in OnRender
        }

        public override void RequestRender(Scene scene, RenderContext renderContext)
        {
            // Not used. Rendering happens in OnRender during scene traversal.
        }

        public override void OnUpdate(double deltaTime)
        {
        }

        public override void OnRender(double deltaTime, RenderContext renderContext)
        {
            Console.WriteLine("comp1 r");
            if (_vertexBuffer == null)
            {
                uint vertexSize = 24; // 6 floats * 4
                _vertexBuffer = renderContext.Factory.CreateBuffer(new Veldrid.BufferDescription((uint)_vertices.Length * vertexSize, Veldrid.BufferUsage.VertexBuffer));
                renderContext.Device.UpdateBuffer(_vertexBuffer, 0, _vertices);

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
                    ResourceLayouts = System.Array.Empty<ResourceLayout>(),
                    Outputs = renderContext.OffscreenFramebuffer.OutputDescription
                };

                _pipeline = renderContext.Factory.CreateGraphicsPipeline(pipelineDesc);
            }

            var cl = renderContext.CommandList;
            cl.SetPipeline(_pipeline);
            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.Draw(3);
        }

        }
}
