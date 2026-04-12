using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyNvg;
using SilkyNvg.Rendering.OpenGL; // 依赖 SilkyNvg.Rendering.OpenGL 包
using SilkyNvg.Text;
using System;
using System.Numerics;

namespace Flat2.Core.Render
{
    public class RenderContext : IDisposable
    {
        private readonly GL _gl;
        private readonly Nvg _nvg;

        public GL Gl => _gl;
        public Nvg Nvg => _nvg; // 暴露 NVG 实例供高级用户直接访问
        public DrawBatcher Batcher { get; private set; }
        private readonly Shader _defaultShader;

        private Vector4 _clearColor = new Vector4(0.2f, 0.2f, 0.25f, 1.0f);
        private Matrix4x4 _projectionView = Matrix4x4.Identity;

        private int _viewportWidth;
        private int _viewportHeight;
        public int ViewportWidth => _viewportWidth;
        public int ViewportHeight => _viewportHeight;
        public bool IsCameraPass { get; set; } = true;
        public void UpdateShaderProjection()
        {
            _defaultShader.Use();
            _defaultShader.SetUniform("uProjectionView", _projectionView);
        }

        public Matrix4x4 ProjectionView
        {
            get => _projectionView;
            set => _projectionView = value;
        }

        // 内置着色器源码保持不变...
        private const string VertexSrc = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec2 aTexCoord;
            layout (location = 2) in vec4 aColor;
            layout (location = 3) in float aTexIndex;
            
            out vec2 vTexCoord;
            out vec4 vColor;
            out float vTexIndex;
            
            uniform mat4 uProjectionView;
            
            void main() {
                vTexCoord = aTexCoord;
                vColor = aColor;
                vTexIndex = aTexIndex;
                gl_Position = uProjectionView * vec4(aPosition, 0.0, 1.0);
            }";

        private const string FragmentSrc = @"
            #version 330 core
            in vec2 vTexCoord;
            in vec4 vColor;
            in float vTexIndex;
            
            out vec4 FragColor;
            
            uniform sampler2D uTextures[16];
            
            void main() {
                int index = int(vTexIndex);
                FragColor = vColor * texture(uTextures[index], vTexCoord);
            }";

        public RenderContext(IWindow window)
        {
            if (window == null) throw new ArgumentNullException(nameof(window));

            _gl = window.CreateOpenGL();
            window.MakeCurrent();

            // 初始化 SilkyNvg，开启抗锯齿
            // 注意：具体初始化语法可能随 SilkyNvg 包版本略有不同
            var nvgFlags = (int)(CreateFlags.Antialias | CreateFlags.StencilStrokes);
            _nvg = Nvg.Create(new OpenGLRenderer((CreateFlags)nvgFlags,_gl));

            _defaultShader = new Shader(_gl, VertexSrc, FragmentSrc);

            // 实例化全新的混合管线 Batcher
            Batcher = new DrawBatcher(_gl, _nvg, _defaultShader, 10000);

            _defaultShader.Use();
            int[] samplers = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
            int loc = _gl.GetUniformLocation(_defaultShader.Handle, "uTextures");
            unsafe
            {
                fixed (int* ptr = samplers)
                {
                    _gl.Uniform1(loc, 16, ptr);
                }
            }
        }

        public void SetClearColor(Vector4 color) => _clearColor = color;

        public void ResizeViewport(int width, int height)
        {
            _viewportWidth = width;
            _viewportHeight = height;
            _gl.Viewport(0, 0, (uint)width, (uint)height);
        }

        // 加载字体的便捷方法
        public int LoadFont(string name, string path)
        {
            return _nvg.CreateFont(name, path);
        }

        public void BeginFrame()
        {
            // 1. 恢复我们自定义 GL 管线的必要状态 (因为上一帧 NanoVG 可能会修改它们)
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            _gl.Enable(EnableCap.DepthTest);
            _gl.Disable(EnableCap.CullFace);

            // 2. 清理屏幕
            _gl.ClearColor(_clearColor.X, _clearColor.Y, _clearColor.Z, _clearColor.W);
            _gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            // 3. 准备自定义管线的 Shader
            UpdateShaderProjection();
            // 4. 开启 NanoVG 帧（第三个参数是像素比例，用于支持高DPI屏幕，默认 1.0f）
            _nvg.BeginFrame(_viewportWidth, _viewportHeight, 1.0f);
        }

        public void EndFrame()
        {
            // 步骤1: 强制把当前积压的 GL Sprite 绘制调用推送到显卡 (作为底层/场景)
            Batcher.Flush();
            _nvg.EndFrame();
            // 步骤2: 结束 NVG 帧，SilkyNvg 会在此时接管状态并执行所有排队的文字和UI绘制 (作为顶层/UI)
        }

        public void Dispose()
        {
            Batcher.Dispose();
            _defaultShader.Dispose();
            _nvg.Dispose();
            _gl.Dispose();
        }
    }
}