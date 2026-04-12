using System.Numerics;

namespace Flat2.Core.Nodes
{
    public class Camera : Node2D
    {
        private float _zoom = 1f;
        private Vector2 _viewportSize = new Vector2(1280, 720);
        private Vector4 _backgroundColor = new Vector4(0.2f, 0.3f, 0.3f, 1.0f);
        /// <summary>
        /// 相机缩放（1 = 正常大小）
        /// </summary>
        public float Zoom
        {
            get => _zoom;
            set { _zoom = Math.Max(0.001f, value); } // 防止为零
        }
        public Vector2 ViewportSize
        {
            get => _viewportSize;
            set => _viewportSize = value;
        }

        /// <summary>
        /// 背景清除颜色（仅主相机有效）
        /// </summary>
        public Vector4 BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public Camera(string name = "Camera") : base(name) { }

        /// <summary>
        /// 计算投影视图矩阵（用于 SpriteBatcher）
        /// </summary>
        public Matrix4x4 GetProjectionViewMatrix()
        {
            // 获取相机的世界变换（Node2D 提供的 LocalToWorld）
            Matrix3x2 worldTransform = LocalToWorld;
            // 计算视图矩阵（相机的逆变换）
            Matrix3x2.Invert(worldTransform, out Matrix3x2 viewMatrix);
            // 正交投影矩阵（中心为相机位置）
            float halfW = ViewportSize.X * 0.5f / Zoom;
            float halfH = ViewportSize.Y * 0.5f / Zoom;
            Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(-halfW, halfW, -halfH, halfH, -1f, 1f);
            // 将 3x2 视图矩阵扩展为 4x4
            Matrix4x4 view4x4 = new Matrix4x4(
                viewMatrix.M11, viewMatrix.M12, 0, viewMatrix.M31,
                viewMatrix.M21, viewMatrix.M22, 0, viewMatrix.M32,
                0, 0, 1, 0,
                0, 0, 0, 1
            );
            // 注意：通常约定为 Projection * View，但这里根据 SpriteBatcher 中着色器的用法（MVP = P * V * M）
            // 为了统一，我们返回 View * Projection（因为 SpriteBatcher 的着色器用 uProjectionView 左乘顶点位置）
            return view4x4 * projection;
        }
    }
}