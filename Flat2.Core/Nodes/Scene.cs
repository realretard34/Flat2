using Flat2.Core.Render;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flat2.Core.Nodes
{
    public class Scene : BaseNode
    {
        public string SceneName { get; init; }
        //主摄像机
        //public Camera? MainCamera { get; set; }
        public override void OnDestroy()
        {
            Console.WriteLine("Scene destroyed");
        }
        public static bool operator ==(Scene? a, Scene? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.SceneName == b.SceneName;
        }

        public static bool operator !=(Scene? a, Scene? b) => !(a == b);

        public override bool Equals(object? obj) => obj is Scene other && this == other;

        public override int GetHashCode() => SceneName?.GetHashCode() ?? 0;

        public override void OnLoad()
        {
            Console.WriteLine("Scene loaded");
        }

        public override void OnRender(double deltaTime,RenderContext ctx)
        {
            //if (MainCamera == null) { 
            //    foreach (var child in Children)
            //    {
            //        if(child is Camera camera)
            //        {
            //            MainCamera = camera;
            //            break;
            //        }
            //    }
            //}
            //应该加一个找摄像机的函数
            foreach (var child in Children)
            {
                if(child.ShouldRenderFromCamera)
                    child.OnRender(deltaTime, ctx);
            }
            ctx.IsCameraPass = true;
            //用上面摄像机的矩阵更新 Shader 的 ProjectionView
            ctx.UpdateShaderProjection(); // 当前 ProjectionView 应该是相机计算后的矩阵
            base.OnRender(deltaTime, ctx); // 走 BaseNode 的逻辑，开始遍历整棵树
            ctx.Batcher.Flush(); // 【重要】把世界层累积的 Sprite 推送到显卡
            ctx.IsCameraPass = false;
            Matrix4x4 oldProj = ctx.ProjectionView; // 暂存相机矩阵
            // 覆盖为基于屏幕大小的正交投影矩阵（左上角为 0,0，右下角为 Width, Height）
            ctx.ProjectionView = Matrix4x4.CreateOrthographicOffCenter(0, ctx.ViewportWidth, ctx.ViewportHeight, 0, -1, 1);
            ctx.UpdateShaderProjection(); // 更新给 Shader
            base.OnRender(deltaTime, ctx); // 再次遍历整棵树，这次只画 UI 节点
            ctx.Batcher.Flush(); // 【重要】把 UI 层的 Sprite 推送到显卡
            ctx.ProjectionView = oldProj; // 恢复相机矩阵供下一帧使用
            ctx.IsCameraPass = true;
        }

        public override void OnUpdate(double deltaTime)
        {
            Console.WriteLine("Scene updated");
            base.OnUpdate(deltaTime);
        }
    }
}
