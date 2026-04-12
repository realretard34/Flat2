using Flat2.Core.Comps;
using Flat2.Core.Render;
using FontStash.NET;
using Flat2.Core.Platform;
namespace Flat2.Core.Nodes
{
    public class BaseNode:IStepObject
    {
        private readonly List<BaseNode> _children = [];
        public BaseNode? Parent { get; private set; }
        public bool ShouldRenderFromCamera { get; set; } = false;
        public IReadOnlyList<BaseNode> Children => _children;
        public IReadOnlyList<BaseComp> Components => _comps;
        public List<BaseComp> _comps = [];
        public bool IsActive { get; set; } = true;
        public string Name { get; set; }= "Node";
        public virtual void OnUpdate(double deltaTime) {
               foreach (var child in Children) 
                     child.OnUpdate(deltaTime);
               foreach (var comp in Components)
                   comp.OnUpdate(deltaTime);
        }
        public virtual void OnLoad() {
                foreach (var child in Children) 
                      child.OnLoad();
                foreach (var comp in Components)
                    comp.OnLoad();
        }
        public virtual void OnRender(double deltaTime,RenderContext ctx) {
            if (this.ShouldRenderFromCamera == ctx.IsCameraPass)
            {
                foreach (var comp in Components)
                {
                    comp.OnRender(deltaTime, ctx);
                }
            }
            foreach (var child in Children)
            {
                child.OnRender(deltaTime, ctx);
            }
        }
        public virtual void OnDestroy() { }
        protected BaseNode(string name = "Node")
        {
            this.Name = name;
        }
        public void AddChild(BaseNode child)
        {
            ArgumentNullException.ThrowIfNull(child);
            child.Parent?.RemoveChild(child);
            child.Parent = this;
            _children.Add(child);
        }
        public void AddComp(BaseComp comp)
        {
            ArgumentNullException.ThrowIfNull(comp);
            _comps.Add(comp);
        }
        public T? GetComponent<T>() where T : BaseComp
        {
            return _comps.OfType<T>().FirstOrDefault();
        }
        public void RemoveChild(BaseNode child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            if (child.Parent != this) return;

            child.Parent = null;
            child.Dispose();
            _ = _children.Remove(child);
        }
        public void RemoveComp(BaseComp comp)
        {
            if (comp == null) throw new ArgumentNullException(nameof(comp));
            if (!_comps.Contains(comp)) return;
            comp.Dispose();
            _ = _comps.Remove(comp);
        }
        public void Dispose()
        {
            foreach (var comp in _comps)
            {
                comp.Dispose();
            }
            _comps.Clear();
            foreach (var child in _children)
            {
                child.Dispose();
            }
            _children.Clear();
            OnDestroy();
        }
    }
}
