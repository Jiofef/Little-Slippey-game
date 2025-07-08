using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static OtherExtension.OtherTools;

namespace OtherExtension
{
    public static class RandomTools
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// 
        /// </summary>
        public static Vector2 RandomVectorInCameraLimits(float minDistanceToPlayer = 0)
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

            return RandomVectorInAlt(ViewportRect, G.Player.GlobalPosition, minDistanceToPlayer);
        }

        public static Vector2I RandomVectorIn(Rect2 rect)
        {
            Vector2I vector = new Vector2I(
            _random.Next((int)rect.Position.X, (int)rect.End.X),
            _random.Next((int)rect.Position.Y, (int)rect.End.Y)
            );

            return vector;
        }

        public static Vector2I RandomVectorIn(Vector2 vec)
        {
            Vector2I vector = new Vector2I(
            _random.Next((int)vec.X),
            _random.Next((int)vec.Y)
            );

            return vector;
        }

        public static Vector2 RandomFloatVectorIn(Rect2 rect)
        {
            Vector2 vector = new Vector2(
                RandomIn(rect.Position.X, rect.End.X),
                RandomIn(rect.Position.Y, rect.End.Y)
                );

            return vector;
        }

        public static Vector2 RandomFloatVectorIn(Vector2 vec)
        {
            Vector2 vector = new Vector2(
                _random.Next((int)vec.X),
                _random.Next((int)vec.Y)
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

        /* - Say my name
         * - Optimizadrot
         * - You're goddamn right */
        /// <summary>
        /// A more advanced alternative for the RandomVectorIn methods
        /// </summary>
        public static Vector2 RandomVectorInAlt(Rect2 rect, Vector2 circleCenter, float circleRadius)
        {
            Vector2 point = new Vector2();
            point.Y = RandomIn(rect.Position.Y, rect.End.Y);

            float yDistToCircleCenter = point.Y - circleCenter.Y;

            // If the circle is on the X line of this position
            if (Mathf.Abs(yDistToCircleCenter) < circleRadius)
            {
                float xChordRange = 2 * Mathf.Sqrt(circleRadius * circleRadius - yDistToCircleCenter * yDistToCircleCenter);

                float xChordStart = circleCenter.X - xChordRange / 2;

                // The origin of the chord must not extend beyond the left edge of the rectangle
                if (xChordStart < rect.Position.X)
                {
                    xChordRange -= rect.Position.X - xChordStart;
                    xChordStart = rect.Position.X;
                }

                if (xChordRange > rect.Size.X)
                    xChordRange = rect.Size.X;

                point.X = RandomIn(rect.Position.X, rect.End.X - xChordRange);

                // Correcting the subtracted chord length from the position
                if (point.X > xChordStart)
                    point.X += xChordRange;
            }
            else
                point.X = RandomIn(rect.Position.X, rect.End.X);

            return point;
        }


        public static bool FiftyFifty()
        {
            return _random.Next(2) == 0;
        }

        public static float RandomIn(Vector2 vector)
        {
            return vector[0] + _random.NextSingle() * (vector[1] - vector[0]);
        }

        /// <summary>
        /// idk why
        /// </summary>
        public static int RandomIn(int min, int max)
        {
            return _random.Next(min, max);
        }
        public static long RandomIn(long min, long max)
        {
            return _random.NextInt64(min, max);
        }
        public static float RandomIn(float min, float max)
        {
            return min + _random.NextSingle() * (max - min);
        }
        public static double RandomIn(double min, double max)
        {
            return min + _random.NextDouble() * (max - min);
        }

        public static int RandomIn(int range)
        {
            return _random.Next(range);
        }
        public static long RandomIn(long range)
        {
            return _random.NextInt64(range);
        }
        public static float RandomIn(float range)
        {
            return _random.NextSingle() * range;
        }
        public static double RandomIn(double range)
        {
            return _random.NextDouble() * range;
        }

        /// <summary>
        /// Chance is the probability of true value
        /// <para>By default, the chance is calculated as a percentage</para>
        /// </summary>
        public static bool TryRand(float chance, float maxValue = 100f)
        {
            return RandomIn(maxValue) <= chance;
        }

        public static T GetRandom<T>(this T[] array)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentException("Array can't be null or empty.");
            }

            return array[_random.Next(array.Length)];
        }

        public static T PickRandomByWeight<T>(IReadOnlyList<T> items, IReadOnlyList<float> weights, bool returnNullIfWeightsAreEmpty = false)
        {
            if (items == null || weights == null || items.Count != weights.Count || items.Count == 0)
                throw new ArgumentException("Items and weights must have the same non-zero length.");

            float totalWeight = weights.Sum();

            if (totalWeight <= 0) return returnNullIfWeightsAreEmpty ? default : items[0];

            float randomValue = (float)(_random.NextDouble() * totalWeight);

            for (int i = 0; i < items.Count; i++)
            {
                if (randomValue < weights[i])
                    return items[i];
                randomValue -= weights[i];
            }

            return items[^1];
        }

		// Random strings
		public static string GetFakeHexString()
		{
		    return "0x" + _random.Next(0, int.MaxValue).ToString("X8");
		}

		public static string GetRandomAsciiString(int length)
		{
			Random rand = new Random();
			return new string(Enumerable.Range(0, length)
				.Select(_ => (char)rand.Next(33, 126))
				.ToArray());
		}

		private static string[] _glitchMessages = {
			"> Allocating...",
			"> NULL REF @ 0x" + GetFakeHexString(),
			"> Buffer overflow",
			"> Writing to stack...",
			"> Injecting DLL",
			"> ERROR: CODE 0x" + GetFakeHexString(),
			"> Stack pointer mismatch"
			};
		public static string GetRandomGlitchLog()
		{
			Random rand = new Random();
			return _glitchMessages[rand.Next(_glitchMessages.Length)];
		}

		public static string GenerateWeirdWord()
		{
			string[] syllables = { "ka", "zo", "ul", "bri", "nek", "dra", "xi", "mur", "tek", "la", "gron", "zu" };
			Random rand = new Random();
			int count = rand.Next(2, 5);
			return string.Concat(Enumerable.Range(0, count).Select(_ => syllables[rand.Next(syllables.Length)]));
		}

		public static string GetRandomGlitchedShit()
		{
			switch (_random.Next(0, 12))
			{
				case 0 or 1 or 10:
					return GetFakeHexString();
				case 2 or 3:
					return GetRandomAsciiString(6);
				case 4 or 5:
					return GetRandomGlitchLog();
				case 6 or 7:
					return GenerateWeirdWord();
				case 8:
					return "[memory leak]";
				case 9:
					return "[corrupted]";
				default:
					return "NaN";
			}
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

    public static class OtherTools
    {
        public static void PrintAnArray(Array array, string delimiter = " | ")
        {
            if (array == null) return;
            GD.Print(ArrayToString(array, delimiter));
        }

        public static void PrintADic<TKey, TValue>(Dictionary<TKey, TValue> dic, string delimiter = " | ", string colon = ": ")
        {
            if (dic == null || dic.Count <= 0) return;
            GD.Print(DicTools.DicToString(dic, delimiter, colon));
        }

        public static string ArrayToString(Array array, string delimiter = " | ")
        {
            string theString = "";
            foreach (var value in array)
                theString += value.ToString() + delimiter;

            // Removing the unnecessary delimiter
            theString = theString.Remove(theString.Length - delimiter.Length, delimiter.Length);

            return theString;
        }

        public static void ActionWithAnIRect(Rect2I rect, Action<int, int> action)
        {
            for (int x = rect.Position.X; x < rect.End.X + 1; x++)
            {
                for (int y = rect.Position.Y; y < rect.End.Y + 1; y++)
                {
                    action(x, y);
                }
            }
        }
    }

    // Note: If you want to create random vectors, use RandomTools
    public static class GeometryTools
    {
        public static Rect2 ResizeRectKeepingCenter(Rect2 rect, Vector2 newSize)
        {
            Vector2 oldCenter = rect.Position + rect.Size / 2;
            rect.Size = newSize;
            rect.Position = oldCenter - rect.Size / 2;
            return rect;
        }

        public static Rect2 ResizeRectWithAbsoluteAnchor(Rect2 rect, Vector2 newSize, Vector2 anchor)
        {
            Vector2 scale = newSize / rect.Size;
            rect.Position = anchor - (anchor - rect.Position) * scale;
            rect.Size = newSize;
            return rect;
        }

        public static Rect2 RectFromCenter(Vector2 center, Vector2 size)
        {
            return new Rect2(center - size / 2, size);
        }

        public static Rect2 RectFromEnd(Vector2 end, Vector2 size)
        {
            return new Rect2(end - size, size);
        }

        public static Rect2 PosEndRect(Vector2 position, Vector2 end)
        {
            return new Rect2(position, end - position);
        }

        public static float Hypotenuse(int side1, int side2)
        {
            return Mathf.Sqrt((side1 * side1) + (side2 * side2));
        }
        public static float Hypotenuse(Vector2 vec)
        {
            return Mathf.Sqrt((vec.X * vec.X) + (vec.Y * vec.Y));
        }

        public static float GetCircleArea(float radius)
        {
            return Mathf.Pi * (radius * radius);
        }

		public static  Vector2 ApplyLimitsToVec(Vector2 vec, Rect2 limits)
		{
			return new Vector2(
				Mathf.Clamp(vec.X, limits.Position.X, limits.Position.X + limits.Size.X),
				Mathf.Clamp(vec.Y, limits.Position.Y, limits.Position.Y + limits.Size.Y)
			);
		}
	}

    public static class MathTools
    {
        public static bool IsInRange(int num, int lower, int upper)
        {
            return num >= lower && num <= upper;
        }
        public static bool IsInRange(float num, float lower, float upper)
        {
            return num >= lower && num <= upper;
        }
        public static bool IsInRange(float num, Vector2 range)
        {
            return num >= range[0] && num <= range[1];
        }

        public static float ClosenessToBorders(float value, float min, float max, float rangeMin = 0, float rangeMax = 0)
        {
            float result = 0;

            if (rangeMin != 0 && value < min + rangeMin)
            {
                result = ClosenessToFloor(value, min, rangeMin);
            }
            else if (rangeMax != 0 && value > max - rangeMax)
            {
                result = ClosenessToCeil(value, max, rangeMax);   
            }

            return result;
        }

        public static float ClosenessTo(float value1, float value2, float range)
        {
            if (range == 0) return 0;

            return Mathf.Max(0, 1 - Mathf.Abs((value1 - value2) / range));
        }

        public static float ClosenessToCeil(float value, float ceil, float range)
        {
            if (range == 0) return value > ceil ? 1 : 0;

            if (value < ceil)
                return Mathf.Max(0, 1 - Mathf.Abs((ceil - value) / range));
            else return 1;
        }

        public static float ClosenessToFloor(float value, float floor, float range)
        {
            if (range == 0) return floor > value ? 1 : 0;

            if (floor < value)
                return Mathf.Max(0, 1 - Mathf.Abs((floor - value) / range));
            else return 1;
        }


        public static float Difference(float value1, float value2)
        {
            return Math.Abs(value1 - value2);
        }

        public static float EaseIn(float t, float power = 2)
        {
            return Mathf.Pow(t, power);
        }

        public static float EaseOut(float t, float power = 2)
        {
            return 1 - Mathf.Pow(1 - t, power);
        }

        public static float EaseInOut(float t, float power = 2)
        {
            if (t < 0.5f)
                return Mathf.Pow(t * 2, power) / 2;
            else
                return 1 - Mathf.Pow((1 - t) * 2, power) / 2;
        }
    }

    public static class DicTools
    {

        public static string DicToString<TKey, TValue>(Dictionary<TKey, TValue> dic, string delimiter = " | ", string colon = ": ")
        {
            string theString = "";
            foreach (var kvp in dic)
            {
                if (!(kvp.Value is Array) && !(kvp.Value is System.Collections.IDictionary))
                    theString += kvp.Key + colon + kvp.Value.ToString() + delimiter;
                else if (kvp.Value is Array array)
                    theString += kvp.Key + colon + ArrayToString(array) + delimiter;
                else if (kvp.Value is System.Collections.IDictionary subDic)
                    theString += kvp.Key + colon + DicToString(subDic, delimiter, colon) + delimiter;
            }
            theString = theString.Remove(theString.Length - delimiter.Length, delimiter.Length);
            return theString;
        }

        public static string DicToString(System.Collections.IDictionary dic, string delimiter = " | ", string colon = ": ")
        {
            string theString = "";
            foreach (System.Collections.DictionaryEntry entry in dic)
            {
                if (!(entry.Value is Array) && !(entry.Value is System.Collections.IDictionary))
                    theString += entry.Key + colon + entry.Value.ToString() + delimiter;
                else if (entry.Value is Array array)
                    theString += entry.Key + colon + ArrayToString(array) + delimiter;
                else if (entry.Value is System.Collections.IDictionary subDic)
                    theString += entry.Key + colon + DicToString(subDic, delimiter, colon) + delimiter;
            }
            theString = theString.Remove(theString.Length - delimiter.Length, delimiter.Length);
            return theString;
        }


        public static bool AreDictionariesEqual<TKey, TValue>(
            Dictionary<TKey, TValue> dict1,
            Dictionary<TKey, TValue> dict2,
            params TKey[] keysToIgnore)
        {
            var ignoreSet = new HashSet<TKey>(keysToIgnore);
            var filteredDict1 = dict1.Where(kvp => !ignoreSet.Contains(kvp.Key))
                                     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var filteredDict2 = dict2.Where(kvp => !ignoreSet.Contains(kvp.Key))
                                     .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            if (filteredDict1.Count != filteredDict2.Count)
                return false;
            foreach (var kvp in filteredDict1)
            {
                if (!filteredDict2.TryGetValue(kvp.Key, out var value2))
                    return false;
                if (kvp.Value is System.Array arr1 && value2 is System.Array arr2)
                {
                    if (arr1.Length != arr2.Length)
                        return false;
                    if (!arr1.Cast<object>().SequenceEqual(arr2.Cast<object>()))
                        return false;
                }
                else if (!EqualityComparer<TValue>.Default.Equals(kvp.Value, value2))
                    return false;
            }
            return true;
        }

        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this Dictionary<TKey, TValue> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var result = new Dictionary<TKey, TValue>(source.Comparer);
            foreach (var kvp in source)
            {
                TValue valueCopy = kvp.Value is ICloneable cloneable ? (TValue)cloneable.Clone() : kvp.Value;
                result.Add(kvp.Key, valueCopy);
            }
            return result;
        }

    }

	public static class FastInstanceCreator
	{

		public const string DEFAULT_RES_SCENES_PATH = "res://Content/Scenes/";
		/// <summary>
		/// The paths starts from "res://Content/Scenes/"
		/// </summary>
		public static PackedScene LoadPackedResScene(string path)
		{
			return GD.Load<PackedScene>(DEFAULT_RES_SCENES_PATH + path);
		}
		public static PackedScene LoadPackedScene(string path)
		{
			return GD.Load<PackedScene>(path);
		}
		/// <summary>
		/// The paths starts from "res://Content/Scenes/"
		/// </summary>
		public static T LoadResScene<T>(string path) where T : Node
		{
			return LoadScene<T>(DEFAULT_RES_SCENES_PATH + path);
		}
		public static Node LoadResScene(string path)
		{
			return LoadScene(DEFAULT_RES_SCENES_PATH + path);
		}


		public static T LoadScene<T>(string path) where T : Node
		{
			return GD.Load<PackedScene>(path).Instantiate<T>();
		}
		public static Node LoadScene(string path)
		{
			return GD.Load<PackedScene>(path).Instantiate();
		}
		public static PackedScene GetPackedFrom(Node node)
		{
			var packedScene = new PackedScene();

			packedScene.Pack(node);

			return packedScene;
		}
	}

    // DON'T LOOK OVER HERE. GOT IT?
    public static class ActionTools
    {
        public class EventWrapper
        {
            public Action Handler;
        }
        public class EventWrapper1A<T>
        {
            public Action<T> Handler;
        }
        public class EventWrapper2A<T1, T2>
        {
            public Action<T1, T2> Handler;
        }
        public class EventWrapper3A<T1, T2, T3>
        {
            public Action<T1, T2, T3> Handler;
        }

        public static void BindEventSafelyTo(EventWrapper eventDelegate, Action action)
        {
            Action handler = action;
            eventDelegate.Handler += handler;
        }
        public static void BindEventSafelyTo<T>(EventWrapper1A<T> eventDelegate, Action<T> action)
        {
            Action<T> handler = action;
            eventDelegate.Handler += handler;
        }
        public static void BindEventSafelyTo<T1, T2>(EventWrapper2A<T1, T2> eventDelegate, Action<T1, T2> action)
        {
            Action<T1, T2> handler = action;
            eventDelegate.Handler += handler;
        }
        public static void BindEventSafelyTo<T1, T2, T3>(EventWrapper3A<T1, T2, T3> eventDelegate, Action<T1, T2, T3> action)
        {
            Action<T1, T2, T3> handler = action;
            eventDelegate.Handler += handler;
        }

        public static void BindEventToNodeSafely(Node node, string methodName, EventWrapper eventDelegate, params Variant[] args)
        {
            Action handler = () => node.Call(methodName, args);
            eventDelegate.Handler += handler;
            node.TreeExiting += () => eventDelegate.Handler -= handler;
        }
        public static void BindEventToNodeSafely<T>(Node node, string methodName, EventWrapper1A<T> eventWrapper, params Variant[] args)
        {
            Action<T> handler = (T arg) =>
            {
                Variant[] callArgs = args.Concat(new Variant[] { Variant.From(arg) }).ToArray();
                node.Call(methodName, callArgs);
            };

            eventWrapper.Handler += handler;
            node.TreeExiting += () => eventWrapper.Handler -= handler;
        }
        public static void BindEventToNodeSafely<T1, T2>(Node node, string methodName, EventWrapper2A<T1, T2> eventDelegate, params Variant[] args)
        {
            Action<T1, T2> handler = (a1, a2) =>
            {
                Variant[] callArgs = args.Concat(new Variant[] { Variant.From(a1), Variant.From(a2) }).ToArray();
                node.Call(methodName, callArgs);
            };
            eventDelegate.Handler += handler;
            node.TreeExiting += () => eventDelegate.Handler -= handler;
        }
        public static void BindEventToNodeSafely<T1, T2, T3>(Node node, string methodName, EventWrapper3A<T1, T2, T3> eventDelegate, params Variant[] args)
        {
            Action<T1, T2, T3> handler = (a1, a2, a3) =>
            {
                Variant[] callArgs = args.Concat(new Variant[] { Variant.From(a1), Variant.From(a2), Variant.From(a3)}).ToArray();
                node.Call(methodName, callArgs);
            };
            eventDelegate.Handler += handler;
            node.TreeExiting += () => eventDelegate.Handler -= handler;
        }

        /// <summary>
        /// only refers to the built-in arguments of the event. The arguments you send here will bind normally.
        /// </summary>
        public static void BindEventToNodeSafelyWithoutArgs<T>(Node node, string methodName, EventWrapper1A<T> eventDelegate, params Variant[] args)
        {
            Action<T> handler = (a1) => node.Call(methodName, args);
            eventDelegate.Handler += handler;
            node.TreeExiting += () => eventDelegate.Handler -= handler;
        }
        /// <summary>
        /// only refers to the built-in arguments of the event. The arguments you send here will bind normally.
        /// </summary>
        public static void BindEventToNodeSafelyWithoutArgs<T1, T2>(Node node, string methodName, EventWrapper2A<T1, T2> eventDelegate, params Variant[] args)
        {
            Action<T1, T2> handler = (a1, a2) => node.Call(methodName, args);
            eventDelegate.Handler += handler;
            node.TreeExiting += () => eventDelegate.Handler -= handler;
        }
        /// <summary>
        /// only refers to the built-in arguments of the event. The arguments you send here will bind normally.
        /// </summary>
        public static void BindEventToNodeSafelyWithoutArgs<T1, T2, T3>(Node node, string methodName, EventWrapper3A<T1, T2, T3> eventDelegate, params Variant[] args)
        {
            Action<T1, T2, T3> handler = (a1, a2, a3) => node.Call(methodName, args);
            eventDelegate.Handler += handler;
            node.TreeExiting += () => eventDelegate.Handler -= handler;
        }
    }

    public static class GodotExtensions
    {
        public static void SetRectOnTileMap(TileMapLayer tileMap, Rect2I rect, int sourceId = -1, Vector2I? atlasCoords = null, int alternativeTile = 0)
        {
            ActionWithAnIRect(rect, (x, y) =>
            {
                tileMap.SetCell(new Vector2I(x, y), sourceId, atlasCoords, alternativeTile);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        public static async Task MoveNodeTo(Node2D node, Vector2 globalPosition, float pxPerFrame = 25f, float accelerationTime = 3f, float maxTime = 15f, float nodeControllabilityCoeff = 1f, float minimumFinishDistancePx = 40f, float controlTimeAfterFinishing = 0f)
        {
            Vector2 velocity = Vector2.Zero;
            float afterFinishingTimer = controlTimeAfterFinishing;
            float timer = 0;
            while (timer < maxTime && (node.GlobalPosition.DistanceTo(globalPosition) > minimumFinishDistancePx || afterFinishingTimer > 0))
            {
                // Timer
                float delta = (float)node.GetProcessDeltaTime();
                timer += delta;

                // Calculating the required speed
                float speedCoeff = timer < accelerationTime ? timer / accelerationTime : 1f;
                velocity = node.GlobalPosition.DirectionTo(globalPosition) * speedCoeff * pxPerFrame;

                // Inherent speed control
                if (node is CharacterBody2D characterBody2D)
                {
                    characterBody2D.Velocity *= 1 - speedCoeff * nodeControllabilityCoeff;
                }

                // Applying the movement
				float distanceSquared = node.GlobalPosition.DistanceSquaredTo(globalPosition);
                node.GlobalPosition = distanceSquared > velocity.LengthSquared() ? node.GlobalPosition + velocity : globalPosition;

                if (distanceSquared > velocity.LengthSquared())
                {
                    node.GlobalPosition += velocity;
                }
                else
                {
                    node.GlobalPosition = globalPosition;
                    afterFinishingTimer -= delta;
                }

                // Waiting for the next frame
                await node.ToSignal(node.GetTree(), "process_frame");
            }
        }
		public static async Task MoveNodeTo(Node2D node, Vector2 globalPosition, MoveNodeProperties properties)
		{
			await MoveNodeTo(node, globalPosition, properties.PxPerFrame, properties.AccelerationTime, properties.MaxTime, properties.NodeControllabilityCoeff, properties.MinimumFinishDistancePx, properties.ControlTimeAfterFinishing);
		}
		public class MoveNodeProperties
		{
			public float PxPerFrame, AccelerationTime, MaxTime, NodeControllabilityCoeff, MinimumFinishDistancePx, ControlTimeAfterFinishing;
			public MoveNodeProperties(float pxPerFrame = 25f, float accelerationTime = 3f, float maxTime = 15f, float nodeControllabilityCoeff = 1f, float minimumFinishDistancePx = 40f, float controlTimeAfterFinishing = 0f)
			{
				PxPerFrame = pxPerFrame;
				AccelerationTime = accelerationTime;
				MaxTime = maxTime;
				NodeControllabilityCoeff = nodeControllabilityCoeff;
				MinimumFinishDistancePx = minimumFinishDistancePx;
				ControlTimeAfterFinishing = controlTimeAfterFinishing;
			}
		}
        public static async Task HideNodeSlowly(CanvasItem node, float duration = 1f, bool disableVisibility = false)
        {
            var tween = node.CreateTween().TweenProperty(node, "modulate", new Color(1, 1, 1, 0), duration);

			try { 
			await node.ToSignal(tween, "finished");} catch (ObjectDisposedException) {return;}
			
            if (disableVisibility)
                node.Visible = false;
        }
        
        public static async Task ShowNodeSlowly(CanvasItem node, float duration = 1f)
        {
            node.Visible = true;
            var tween = node.CreateTween().TweenProperty(node, "modulate", new Color(1, 1, 1, 1), duration);

			try { 
			await node.ToSignal(tween, "finished");} catch (ObjectDisposedException) {return;}
        }
        public static void ActionWithANodeTree(Node treeRoot, Action<Node> action)
        {
            void StackOverflow(Node parent)
            {
                foreach (Node child in treeRoot.GetChildren())
                {
                    action(child);
                    StackOverflow(child);
                }
            }

            StackOverflow(treeRoot);
        }

        /// <summary>
        /// Returns the ancestor of the represented type, if it exists. If not, returns null
        /// </summary>
        public static T FindParent<T>(Node node) where T : class
        {
            while (node != null)
            {
                if (node is T result)
                {
                    return result;
                }

                node = node.GetParent();
            }

            return default(T);
        }

        public static void DoForEachParent(Action<Node> action, Node node)
        {
            node = node.GetParent();

            while (node != null)
            {
                action(node);

                node = node.GetParent();
            }
        }

        public static async Task WaitForFrame()
        {
            await G.SceneTree.ToSignal(G.SceneTree, "process_frame");
        }

        public static Color GetGlobalModulateOf(CanvasItem item)
        {
            Node currentNode = item;
            Color resultModulate = new Color(1, 1, 1, 1);

            while (currentNode != null)
            {
                if (currentNode is CanvasItem canvasItem)
                    resultModulate *= canvasItem.Modulate;

                currentNode = currentNode.GetParent();
            }

            return resultModulate;
        }

        public static string GetTreePretty(Node node, string indent = "", bool last = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(indent);
            if (last)
            {
                sb.Append("└─ ");
                indent += "   ";
            }
            else
            {
                sb.Append("├─ ");
                indent += "│  ";
            }
            sb.AppendLine(node.Name);

            int childCount = node.GetChildCount();
            for (int i = 0; i < childCount; i++)
            {
                Node child = node.GetChild(i);
                bool isLast = i == childCount - 1;
                sb.Append(GetTreePretty(child, indent, isLast));
            }

            return sb.ToString();
        }

        /// <summary>
        /// ID exists in case the nodes can be more than one and not the first of them is needed
        /// </summary>
        public static Type FindNodeOfType<Type>(Node parent, int id = 0) where Type : class
        {
            var children = parent.GetChildren().OfType<Type>().ToArray();
            if (children.Count() > id)
                return children[id];

            return null;
        }
    }

    // A-Num
    public static class DeterministicRandom
    {

    }
}