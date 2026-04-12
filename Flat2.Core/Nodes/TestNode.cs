using Flat2.Core.Render;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Flat2.Core.Nodes
{
    public class TestNode:BaseNode
    {
        public override void OnRender(double s,RenderContext ctx)
        {
            if (ShouldRenderFromCamera == ctx.IsCameraPass)
            {
                Console.WriteLine("渲染ui中");
                ctx.Batcher.DrawRect(new Vector2(0, 0), new Vector2(200, 200), new Vector4(1,0,1,1));

                // 在这里可以手动调用 ctx.Batcher.DrawSprite 等逻辑
            }
            else
                {
                Console.WriteLine("渲染摄像机中");
                // 在这里可以手动调用 ctx.Batcher.DrawSprite 等逻辑
            }
            // 【必须写】这样才能把遍历链条传导给子节点
            base.OnRender(s, ctx);
        }
    }
}
