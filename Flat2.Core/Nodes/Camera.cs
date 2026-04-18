using System.Numerics;

namespace Flat2.Core.Nodes
{
    public class Camera
    {
        public Matrix4x4 View { get; set; } = Matrix4x4.Identity;
        public Matrix4x4 Projection { get; set; } = Matrix4x4.Identity;

        public Matrix4x4 ViewProjection => View * Projection;

        public void SetViewLookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            View = Matrix4x4.CreateLookAt(eye, target, up);
        }

        public void SetOrthographic(float width, float height, float zNear, float zFar)
        {
            Projection = Matrix4x4.CreateOrthographic(width, height, zNear, zFar);
        }

        public void SetPerspectiveFieldOfView(float fov, float aspectRatio, float zNear, float zFar)
        {
            Projection = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspectRatio, zNear, zFar);
        }
    }
}
