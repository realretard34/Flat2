using Silk.NET.OpenGL;
using SilkyNvg;
using SilkyNvg.Graphics;
using SilkyNvg.Paths;
using SilkyNvg.Text;
using System;
using System.Numerics;

namespace Flat2.Core.Render
{
    public unsafe class DrawBatcher : IDisposable
    {
        private readonly GL _gl;
        private readonly Nvg _nvg;
        private readonly Shader _shader;
        private readonly uint _vao, _vbo, _ebo;
        private readonly SpriteVertex[] _vertices;
        private int _spriteCount = 0;
        private readonly int _maxSprites;
        private readonly uint[] _textureSlots = new uint[16];
        private int _textureCount = 0;

        public DrawBatcher(GL gl, Nvg nvg, Shader shader, int maxSprites = 10000)
        {
            _gl = gl;
            _nvg = nvg;
            _shader = shader;
            _maxSprites = maxSprites;
            _vertices = new SpriteVertex[maxSprites * 4];

            _vao = _gl.GenVertexArray();
            _vbo = _gl.GenBuffer();
            _ebo = _gl.GenBuffer();

            // VBO / EBO 初始化代码保持不变...
            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(_vertices.Length * sizeof(SpriteVertex)), (void*)0, BufferUsageARB.DynamicDraw);
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
            int slot = GetTextureSlot(textureId);
            if (slot == -1 || _spriteCount >= _maxSprites)
            {
                Flush();
                slot = GetTextureSlot(textureId);
            }

            float hw = size.X * 0.5f;
            float hh = size.Y * 0.5f;

            Span<Vector2> localCorners = stackalloc Vector2[4] {
                new Vector2(-hw, -hh), new Vector2(hw, -hh),
                new Vector2(hw, hh),   new Vector2(-hw, hh)
            };

            int offset = _spriteCount * 4;
            for (int i = 0; i < 4; i++)
            {
                Vector2 worldPos = Vector2.Transform(localCorners[i], worldMatrix);
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
            for (int i = 0; i < _textureCount; i++)
            {
                if (_textureSlots[i] == textureId) return i;
            }
            if (_textureCount < 16)
            {
                _textureSlots[_textureCount] = textureId;
                return _textureCount++;
            }
            return -1;
        }

        public void Flush()
        {
            Console.WriteLine("结束一帧");
            if (_spriteCount == 0) return;

            // 确保我们的 Shader 处于活跃状态 (防范中间帧 NVG 状态污染)
            _shader.Use();

            for (int i = 0; i < _textureCount; i++)
            {
                _gl.ActiveTexture(TextureUnit.Texture0 + i);
                _gl.BindTexture(TextureTarget.Texture2D, _textureSlots[i]);
            }

            _gl.BindVertexArray(_vao);
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            fixed (SpriteVertex* ptr = _vertices)
            {
                _gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0, (nuint)(_spriteCount * 4 * sizeof(SpriteVertex)), ptr);
            }

            _gl.DrawElements(PrimitiveType.Triangles, (uint)(_spriteCount * 6), DrawElementsType.UnsignedInt, (void*)0);

            _spriteCount = 0;
            _textureCount = 0;
            _gl.BindVertexArray(0);
        }
        /// <summary>
        /// 绘制文本（屏幕空间坐标）
        /// </summary>
        public void DrawText(string text, Vector2 screenPos, string fontName, float fontSize, Vector4 color)
        {
            _nvg.FontFace(fontName);
            _nvg.FontSize(fontSize);
            _nvg.FillColour(VectorToNvgColor(color));
            // 默认设置为左上角对齐，符合大部分 2D 游戏引擎的直觉
            _nvg.TextAlign((SilkyNvg.Text.Align.Left | SilkyNvg.Text.Align.Top));
            _nvg.Text(screenPos.X, screenPos.Y, text);
        }
        /// <summary>
        /// 绘制矢量矩形
        /// </summary>
        public void DrawRect(Vector2 screenPos, Vector2 size, Vector4 color)
        {
            _nvg.BeginPath();
            _nvg.Rect(screenPos.X, screenPos.Y, size.X, size.Y);
            _nvg.FillColour(VectorToNvgColor(color));
            _nvg.Fill();
        }
        /// <summary>
        /// 绘制矢量圆形
        /// </summary>
        public void DrawCircle(Vector2 screenCenter, float radius, Vector4 color)
        {
            _nvg.BeginPath();
            _nvg.Circle(screenCenter.X, screenCenter.Y, radius);
            _nvg.FillColour(VectorToNvgColor(color));
            _nvg.Fill();
        }
        private Colour VectorToNvgColor(Vector4 v)
        {
            return new Colour(v.X, v.Y, v.Z, v.W);
        }
        public void Dispose()
        {
            _gl.DeleteVertexArray(_vao);
            _gl.DeleteBuffer(_vbo);
            _gl.DeleteBuffer(_ebo);
        }
    }
}