using System.Numerics;

namespace Flat2.Core.Nodes
{
    public class Node2D(string name = "Node2D") : BaseNode(name)
    {
        private Vector2 _position;
        private float _rotation;
        private Vector2 _scale = Vector2.One;

        public Vector2 Position
        {
            get => _position;
            set { _position = value; MarkTransformDirty(); }
        }

        public float Rotation
        {
            get => _rotation;
            set { _rotation = value; MarkTransformDirty(); }
        }

        public Vector2 Scale
        {
            get => _scale;
            set { _scale = value; MarkTransformDirty(); }
        }

        private bool _transformDirty = true;
        private Matrix3x2 _localToParent = Matrix3x2.Identity;
        private Matrix3x2 _localToWorld = Matrix3x2.Identity;

        public Matrix3x2 LocalToParent
        {
            get
            {
                if (_transformDirty) UpdateLocalTransform();
                return _localToParent;
            }
        }

        public Matrix3x2 LocalToWorld
        {
            get
            {
                if (_transformDirty || (Parent is Node2D p && p._transformDirty))
                    UpdateWorldTransform();
                return _localToWorld;
            }
        }

        private void MarkTransformDirty()
        {
            _transformDirty = true;
            foreach (var child in Children)
                if (child is Node2D n2d)
                    n2d.MarkTransformDirty();
        }

        private void UpdateLocalTransform()
        {
            _localToParent = Matrix3x2.CreateScale(_scale) *
                             Matrix3x2.CreateRotation(_rotation) *
                             Matrix3x2.CreateTranslation(_position);
            _transformDirty = false;
        }

        private void UpdateWorldTransform()
        {
            UpdateLocalTransform();
            if (Parent is Node2D parent)
                _localToWorld = _localToParent * parent.LocalToWorld;
            else
                _localToWorld = _localToParent;
        }

        // 覆盖更新逻辑，可添加组件更新
        public override void OnUpdate(double deltaTime)
        {
            base.OnUpdate(deltaTime);
            foreach (var comp in _comps)
            {
                comp.OnUpdate(deltaTime);
            }
        }
    }
}