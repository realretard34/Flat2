using Flat2.Core.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Compontent
{
    public class BaseComp
    {
        /// <summary>
        /// 当节点被更新时调用。deltaTime参数表示自上次更新以来的时间（以秒为单位）。子类可以重写此方法来实现特定的更新逻辑。
        /// </summary>
        /// <param name="deltaTime"></param>
        public virtual void OnUpdate(double deltaTime) { }
        /// <summary>
        /// 首次被加载时调用。子类可以重写此方法来执行初始化逻辑，例如加载资源、设置初始状态等。
        /// </summary>
        public virtual void OnLoad() { }
        /// <summary>
        /// 当节点需要渲染时调用。子类可以重写此方法来实现特定的渲染逻辑。
        /// </summary>
        public virtual void OnRender(double deltaTime,RenderContext ctx) { }
        /// <summary>
        /// 当节点被销毁时调用。子类可以重写此方法来执行清理逻辑，例如释放资源、取消订阅事件等。
        /// </summary>
        public virtual void OnDestroy() { }
        /// <summary>
        /// 标记节点是否处于活动状态。只有当节点及其所有父节点都处于活动状态时，才会调用OnUpdate、OnRender等方法。子类可以根据需要重写此属性来实现特定的激活逻辑。
        /// </summary>
        public bool isActive { get; set; }
    }
}
