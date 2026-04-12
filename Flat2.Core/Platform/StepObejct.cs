using Flat2.Core.Render;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flat2.Core.Platform
{
    public interface IStepObject : IDisposable
    {

        /// <summary>
        /// 当节点被更新时调用。deltaTime参数表示自上次更新以来的时间（以秒为单位）。子类可以重写此方法来实现特定的更新逻辑。
        /// </summary>
        /// <param name="deltaTime"></param>
        public void OnUpdate(double deltaTime);
        /// <summary>
        /// 首次被加载时调用。子类可以重写此方法来执行初始化逻辑，例如加载资源、设置初始状态等。
        /// </summary>
        public void OnLoad();
        /// <summary>
        /// 当节点需要渲染时调用。子类可以重写此方法来实现特定的渲染逻辑。
        /// </summary>
        public void OnRender(double deltaTime, RenderContext ctx);
        /// <summary>
        /// 当节点被销毁时调用。子类可以重写此方法来执行清理逻辑，例如释放资源、取消订阅事件等。
        /// </summary>
        public void OnDestroy();
        public void Dispose()
        {
            OnDestroy();
        }
    }
}
