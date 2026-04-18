using Flat2.Core.Nodes;

namespace Flat2.Core.Renderer
{
    public class RenderFPL
    {
        public List<RenderPass> Passes { get; set; } = [];
        public void OnRender(RenderContext renderContext,Scene scene,double deltaTime)
        {
            renderContext.Begin();
            scene.OnRender(deltaTime, renderContext);
            foreach (var pass in Passes)
            {
                pass.OnRender(renderContext);
            }
            renderContext.Submit();
        }
        public void OnLoad(RenderContext renderContext)
        {
            foreach (var pass in Passes)
            {
                pass.OnLoad(renderContext);
            }
        }
    }
}
