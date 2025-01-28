using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

// is neither a script nor a singleton, but a namespace. Since it is one of a kind, I don't want to create a separate folder for it
namespace OtherExtension
{
    public static class RandomTools
    {
        private static readonly Random _random = new Random();

        public static Vector2 RandomVectorInCameraBorders(float minDistanceToPlayer = 0)
        {
            Vector2 vector;

            vector = RandomVectorAt(G.CameraLimits.GetCenter(), G.CameraLimits.Size, minDistanceToPlayer);
            int attempts = 20;
            do
            {
                attempts--;
                vector = new Vector2(
                _random.Next((int)G.CameraLimits.Position.X, (int)G.CameraLimits.End.X),
                _random.Next((int)G.CameraLimits.Position.Y, (int)G.CameraLimits.End.Y)
                );
            }
            while (attempts > 0 && vector.DistanceTo(G.Player.GlobalPosition) < minDistanceToPlayer);
            return vector;
        }

        public static Vector2 RandomVectorInViewport(CanvasItem any2DNodeInScene, float minDistanceToPlayer = 0)
        {
            var CanvasTransfrom = any2DNodeInScene.GetCanvasTransform();

            Rect2 ViewportRect = new Rect2(-CanvasTransfrom.Origin, any2DNodeInScene.GetViewportRect().Size / CanvasTransfrom.Scale);

            return RandomVectorAt(ViewportRect.GetCenter(), ViewportRect.Size, minDistanceToPlayer);
        }

        public static Vector2 RandomVectorIn(Rect2 rect)
        {
            Vector2 vector = new Vector2(
            _random.Next((int)rect.Position.X, (int)rect.End.X),
            _random.Next((int)rect.Position.Y, (int)rect.End.Y)
            );

            return vector;
        }

        public static Vector2 RandomVectorIn(Rect2 rect, Vector2 point, float minDistanceToPoint, float? maxDistanceToPoint = null)
        {
            Vector2 vector;
            int attempts = 20;

            float distanceToPoint;
            do
            {
                attempts--;
                if (maxDistanceToPoint == null)
                {
                    vector = new Vector2(
                    _random.Next((int)rect.Position.X, (int)rect.End.X),
                    _random.Next((int)rect.Position.Y, (int)rect.End.Y)
                    );
                }
                else
                {
                    vector = point + RandomVectorInCircle((float)maxDistanceToPoint, minDistanceToPoint);
                }
                distanceToPoint = vector.DistanceTo(point);
            }
            while (attempts > 0 && (distanceToPoint < minDistanceToPoint || !rect.HasPoint(vector)));

            return vector;
        }

        public static Vector2 RandomVectorInCircle(float radius)
        {
            Vector2 vector = new Vector2(vector.X = _random.NextSingle() * radius, 0)
                .Rotated(_random.NextSingle() * 6.283f);

            return vector;
        }

        public static Vector2 RandomVectorInCircle(float radius, float minLength = 0)
        {
            Vector2 vector = new Vector2(vector.X = minLength + _random.NextSingle() * (radius - minLength), 0)
                .Rotated(_random.NextSingle() * 6.283f);

            return vector;
        }

        public static Vector2 RandomVectorAt(Vector2 center, Vector2 rectSize, float minDistanceToCenter = 0)
        {
            Vector2 vector = new Vector2();
            float angle = _random.NextSingle() * MathF.Tau;


            //float maxRadiusX = rectSize.X / 2 / MathF.Abs(MathF.Cos(angle));
            //float maxRadiusY = rectSize.Y / 2 / MathF.Abs(MathF.Sin(angle));
            //float maxRadius = MathF.Min(maxRadiusX, maxRadiusY);

            //float randomRadius = minDistanceToCenter + _random.NextSingle() * (maxRadius - minDistanceToCenter);

            float randomRadius = minDistanceToCenter + _random.NextSingle() * (MathF.Min(rectSize.X / 2 / MathF.Abs(MathF.Cos(angle)), rectSize.Y / 2 / MathF.Abs(MathF.Sin(angle))) - minDistanceToCenter);

            vector.X = randomRadius * MathF.Cos(angle);
            vector.Y = randomRadius * MathF.Sin(angle);

            vector += center;


            return vector;
        }

        public static bool FiftyFifty()
        {
            return _random.Next(2) == 0;
        }

        public static float RandomIn(Vector2 vector)
        {
            return vector[0] + _random.NextSingle() * (vector[1] - vector[0]);
        }

        public static float RandomIn(float min, float max)
        {
            return min + _random.NextSingle() * (max - min);
        }
    }
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