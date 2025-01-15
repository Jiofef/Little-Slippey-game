using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

// is neither a script nor a singleton, but a namespace. Since it is one of a kind, I don't want to create a separate folder for it
namespace OtherExtension
{
    /// <summary>
    /// Not related to Godot nodes. Created for storing organized objects with the ability to add children to the objects.
    /// </summary>
    public class TreeNode<T> : ICloneable
    {
        public object Clone()
        {
            // Cloning the value
            T ValueClone;
            if (Value is ICloneable cValue)
            {
                ValueClone = (T)cValue.Clone();
            }
            else ValueClone = Value;

            var clone = new TreeNode<T>(ValueClone);

            // Cloning the children
            foreach (var child in _children)
                clone.AddChild((TreeNode<T>)child.Clone());

            return clone;
        }

        public T Value { get; set; }

        [JsonInclude]
        private List<TreeNode<T>> _children { get; set; }

        private TreeNode<T> _parent { get; set; }

        public TreeNode(T value)
        {
            Value = value;
            _children = new List<TreeNode<T>>();
        }

        public void AddChild(TreeNode<T> child)
        {
            child._parent = this;
            _children.Add(child);
        }

        public TreeNode<T> AddChild(T value)
        {
            var childItem = new TreeNode<T>(value);
            childItem._parent = this;

            _children.Add(childItem);

            return childItem;
        }

        public TreeNode<T> GetChild(int index)
        {
            return _children[index];
        }

        public List<TreeNode<T>> GetChildren()
        {
            return _children;
        }

        public TreeNode<T> GetParent()
        {
            return _parent;
        }

        /// <summary>
        /// Removes an object from the parent's list if the parent exists
        /// </summary>
        public void Free()
        {
            if (_parent != null)
            {
                _parent._children.Remove(this);
                _parent = null;
            }
        }
    }
}