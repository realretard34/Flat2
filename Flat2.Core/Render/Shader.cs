using Silk.NET.OpenGL;
using System;

namespace Flat2.Core.Render
{
    public class Shader : IDisposable
    {
        public uint Handle { get; private set; }
        private readonly GL _gl;

        public Shader(GL gl, string vertexSource, string fragmentSource)
        {
            _gl = gl;
            uint vertex = LoadShader(ShaderType.VertexShader, vertexSource);
            uint fragment = LoadShader(ShaderType.FragmentShader, fragmentSource);
            Handle = _gl.CreateProgram();
            _gl.AttachShader(Handle, vertex);
            _gl.AttachShader(Handle, fragment);
            _gl.LinkProgram(Handle);
            _gl.GetProgram(Handle, ProgramPropertyARB.LinkStatus, out int status);
            if (status == 0)
                throw new Exception($"Program link failed: {_gl.GetProgramInfoLog(Handle)}");
            _gl.DetachShader(Handle, vertex);
            _gl.DetachShader(Handle, fragment);
            _gl.DeleteShader(vertex);
            _gl.DeleteShader(fragment);
        }

        private uint LoadShader(ShaderType type, string source)
        {
            uint shader = _gl.CreateShader(type);
            _gl.ShaderSource(shader, source);
            _gl.CompileShader(shader);
            _gl.GetShader(shader, ShaderParameterName.CompileStatus, out int status);
            if (status == 0)
                throw new Exception($"{type} compile failed: {_gl.GetShaderInfoLog(shader)}");
            return shader;
        }

        public void Use() => _gl.UseProgram(Handle);

        public void SetUniform(string name, int value) => _gl.Uniform1(GetUniformLocation(name), value);
        public void SetUniform(string name, float value) => _gl.Uniform1(GetUniformLocation(name), value);
        public void SetUniform(string name, System.Numerics.Matrix4x4 value)
        {
            unsafe
            {
                _gl.UniformMatrix4(GetUniformLocation(name), 1, false, (float*)&value);
            }
        }

        private int GetUniformLocation(string name) => _gl.GetUniformLocation(Handle, name);

        public void Dispose() => _gl.DeleteProgram(Handle);
    }
}