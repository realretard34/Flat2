using Silk.NET.OpenGL;
using System;
using System.Numerics;

namespace Flat2.Core.Render
{
    public unsafe class SpriteBatcher : IDisposable
    {
        private readonly GL _gl;
        private readonly Shader _shader;

        private readonly uint _vao, _vbo, _ebo;
        private readonly SpriteVertex[] _vertices;
        private int _spriteCount = 0;
        private readonly int _maxSprites;

        // 纹理插槽系统：支持单次 DrawCall 使用 16 张贴图
        private readonly uint[] _textureSlots = new uint[16];
        private int _textureCount = 0;

        public SpriteBatcher(GL gl, Shader shader, int maxSprites = 10000)
        {
            _gl = gl;
            _shader = shader;
            _maxSprites = maxSprites;
            _vertices = new SpriteVertex[maxSprites * 4];

            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();
            _ebo = _gl.GenBuffer();

            _gl.BindVertexArray(_vao);

            // 1. 初始化 VBO (修复 0xC0000005 崩溃)
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            // 【关键修复】：显式使用 (void*)0 代替 null，明确告诉 Silk.NET 我们在操作底层指针
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_vertices.Length * sizeof(SpriteVertex)), (void*)0, BufferUsageARB.DynamicDraw);

            // 2. 初始化 EBO (索引缓冲区)
            uint[] indices = new uint[maxSprites * 6];
            for (uint i = 0; i < maxSprites; i++)
            {
                uint offset = i * 4;
                indices[i * 6 + 0] = offset + 0;
                indices[i * 6 + 1] = offset + 1;
                indices[i * 6 + 2] = offset + 2;
                indices[i * 6 + 3] = offset + 2;
                indices[i * 6 + 4] = offset + 3;
                indices[i * 6 + 5] = offset + 0;
            }

            _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (uint* pIndices = indices)
            {
                _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(indices.Length * sizeof(uint)), pIndices, BufferUsageARB.StaticDraw);
            }

            // 3. 配置顶点属性指针
            uint stride = (uint)sizeof(SpriteVertex);
            _gl.EnableVertexAttribArray(0);
            _gl.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, (void*)0);

            _gl.EnableVertexAttribArray(1);
            _gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, (void*)8);

            _gl.EnableVertexAttribArray(2);
            _gl.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, stride, (void*)16);

            _gl.EnableVertexAttribArray(3);
            _gl.VertexAttribPointer(3, 1, VertexAttribPointerType.Float, false, stride, (void*)32);

            _gl.BindVertexArray(0);
        }

        public void DrawSprite(uint textureId, Matrix3x2 worldMatrix, Vector2 size, Vector4 color, Vector4 sourceRect)
        {
            // 检查缓存是否已满，或者纹理插槽是否耗尽
            int slot = GetTextureSlot(textureId);
            if (slot == -1 || _spriteCount >= _maxSprites)
            {
                Flush();
                slot = GetTextureSlot(textureId);
            }

            // 零分配计算 4 个顶点
            float hw = size.X * 0.5f;
            float hh = size.Y * 0.5f;

            Span<Vector2> localCorners = stackalloc Vector2[4] {
                new Vector2(-hw, -hh), // 左上
                new Vector2(hw, -hh),  // 右上
                new Vector2(hw, hh),   // 右下
                new Vector2(-hw, hh)   // 左下
            };

            int offset = _spriteCount * 4;
            for (int i = 0; i < 4; i++)
            {
                // SIMD 世界坐标转换
                Vector2 worldPos = Vector2.Transform(localCorners[i], worldMatrix);

                // 简单的 UV 映射 (假设 sourceRect 是归一化的 0~1)
                Vector2 uv = i switch
                {
                    0 => new Vector2(sourceRect.X, sourceRect.Y),
                    1 => new Vector2(sourceRect.Z, sourceRect.Y),
                    2 => new Vector2(sourceRect.Z, sourceRect.W),
                    _ => new Vector2(sourceRect.X, sourceRect.W)
                };

                _vertices[offset + i] = new SpriteVertex(worldPos, uv, color, slot);
            }

            _spriteCount++;
        }

        private int GetTextureSlot(uint textureId)
        {
            // 如果已经在插槽中，直接返回
            for (int i = 0; i < _textureCount; i++)
            {
                if (_textureSlots[i] == textureId) return i;
            }

            // 如果有空位，加入插槽
            if (_textureCount < 16)
            {
                _textureSlots[_textureCount] = textureId;
                return _textureCount++;
            }

            // 插槽已满
            return -1;
        }

        public void Flush()
        {
            if (_spriteCount == 0) return;

            // 1. 绑定用到的所有纹理
            for (int i = 0; i < _textureCount; i++)
            {
                _gl.ActiveTexture(TextureUnit.Texture0 + i);
                _gl.BindTexture(TextureTarget.Texture2D, _textureSlots[i]);
            }

            // 2. 更新 VBO 数据并绘制
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            fixed (SpriteVertex* ptr = _vertices)
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(_spriteCount * 4 * sizeof(SpriteVertex)), ptr);
            }

            _gl.DrawElements(PrimitiveType.Triangles, (uint)(_spriteCount * 6), DrawElementsType.UnsignedInt, (void*)0);

            // 3. 重置状态
            _spriteCount = 0;
            _textureCount = 0;
            _gl.BindVertexArray(0);
        }

        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
    }
}