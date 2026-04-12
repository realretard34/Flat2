using Flat2.Core.Platform;
using Flat2.Core.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Comps
{
    public class BaseComp : IStepObject
    {
        /// <param name="deltaTime"></param>
        public virtual void OnUpdate(double deltaTime) { }
        public virtual void OnLoad() { }
        /// <summary>
        /// 当节点需要渲染时调用。子类可以重写此方法来实现特定的渲染逻辑。
        /// </summary>
        public virtual void OnRender(double deltaTime,RenderContext ctx) { }
        /// <summary>
        /// 当节点被销毁时调用。子类可以重写此方法来执行清理逻辑，例如释放资源、取消订阅事件等。
        /// </summary>
        public virtual void OnDestroy() { }

        public void Dispose()
        {
            OnDestroy();
        }

        /// <summary>
        /// 标记节点是否处于活动状态。只有当节点及其所有父节点都处于活动状态时，才会调用OnUpdate、OnRender等方法。子类可以根据需要重写此属性来实现特定的激活逻辑。
        /// </summary>
        public bool IsActive { get; set; }
    }
}
