using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Numerics;

namespace Flat2.Core.Render
{
    public class RenderContext : IDisposable
    {
        private readonly GL _gl;
        public SpriteBatcher Batcher { get; private set; }
        private readonly Shader _defaultShader;

        private Vector4 _clearColor = new Vector4(0.2f, 0.2f, 0.25f, 1.0f);
        private Matrix4x4 _projectionView = Matrix4x4.Identity;

        // 外部暴露的属性
        public Matrix4x4 ProjectionView
        {
            get => _projectionView;
            set => _projectionView = value;
        }

        // 内置着色器源码：确保与 SpriteVertex 和 Camera 系统对应
        private const string VertexSrc = @"
            #version 330 core
            layout (location = 0) in vec2 aPosition;
            layout (location = 1) in vec2 aTexCoord;
            layout (location = 2) in vec4 aColor;
            layout (location = 3) in float aTexIndex;
            
            out vec2 vTexCoord;
            out vec4 vColor;
            out float vTexIndex;
            
            uniform mat4 uProjectionView; // 相机和屏幕的组合矩阵
            
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

            // 获取并激活上下文
            _gl = window.CreateOpenGL();
            window.MakeCurrent();

            // 基础状态配置
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // 初始化底层设施
            _defaultShader = new Shader(_gl, VertexSrc, FragmentSrc);
            Batcher = new SpriteBatcher(_gl, _defaultShader, 10000);

            // 预先初始化 Shader 中的采样器数组 (告知显卡 0-15 号插槽分别对应哪些纹理单元)
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
            _gl.Viewport(0, 0, (uint)width, (uint)height);
        }

        public void BeginFrame()
        {
            // 1. 清理屏幕
            _gl.ClearColor(_clearColor.X, _clearColor.Y, _clearColor.Z, _clearColor.W);
            _gl.Clear(ClearBufferMask.ColorBufferBit);

            // 2. 应用相机矩阵到全局 Shader
            _defaultShader.Use();
            _defaultShader.SetUniform("uProjectionView", _projectionView);
        }

        public void EndFrame()
        {
            // 确保最后一批次的数据被推送到显卡
            Batcher.Flush();
        }

        public void Dispose()
        {
            Batcher.Dispose();
            _defaultShader.Dispose();
            _gl.Dispose();
        }
    }
}