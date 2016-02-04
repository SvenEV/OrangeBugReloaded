using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Represents a rectangle with integer X and Y values
    /// and non-negative integer width and height.
    /// It is assumed that the X axis points to the right
    /// and the Y axis points upwards.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [JsonConverter(typeof(RectangleConverter))]
    public struct Rectangle : IEnumerable<Point>
    {
        /// <summary>
        /// A rectangle with zero size and X and Y set to zero.
        /// </summary>
        public static Rectangle Zero { get; } = new Rectangle(0, 0, 0, 0);

        /// <summary>
        /// The X coordinate of the left edge.
        /// </summary>
        [JsonProperty]
        public int Left { get; }

        /// <summary>
        /// The Y coordinate of the bottom edge.
        /// </summary>
        [JsonProperty]
        public int Bottom { get; }

        /// <summary>
        /// The X coordinate of the right edge that is the sum
        /// of <see cref="Left"/> and <see cref="Width"/>.
        /// </summary>
        public int Right => Left + Width;

        /// <summary>
        /// The Y coordinate of the top edge that is the sum
        /// of <see cref="Bottom"/> and <see cref="Height"/>.
        /// </summary>
        public int Top => Bottom + Height;

        /// <summary>
        /// The non-negative width.
        /// </summary>
        [JsonProperty]
        public int Width { get; }

        /// <summary>
        /// The non-negative height.
        /// </summary>
        [JsonProperty]
        public int Height { get; }

        /// <summary>
        /// Size of the <see cref="Rectangle"/>.
        /// </summary>
        public Point Size => new Point(Width, Height);

        /// <summary>
        /// Bottom left corner <see cref="Point"/>.
        /// </summary>
        public Point BottomLeft => new Point(Left, Bottom);

        /// <summary>
        /// Bottom right corner <see cref="Point"/>.
        /// </summary>
        public Point BottomRight => new Point(Left + Width, Bottom);

        /// <summary>
        /// Top left corner <see cref="Point"/>.
        /// </summary>
        public Point TopLeft => new Point(Left, Bottom + Height);

        /// <summary>
        /// Top right corner <see cref="Point"/>.
        /// </summary>
        public Point TopRight => new Point(Left + Width, Bottom + Height);

        /// <summary>
        /// Initializes a new <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="x">Left edge</param>
        /// <param name="y">Bottom edge</param>
        /// <param name="width">Width (non-negative)</param>
        /// <param name="height">Height (non-negative)</param>
        public Rectangle(int x, int y, int width, int height) : this()
        {
            if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 0) throw new ArgumentOutOfRangeException(nameof(height));

            Left = x;
            Bottom = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Initializes a new <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="position">Position (left, bottom)</param>
        /// <param name="size">Size (non-negative)</param>
        public Rectangle(Point position, Point size)
        {
            if (size.X < 0 || size.Y < 0) throw new ArgumentOutOfRangeException(nameof(size));

            Left = position.X;
            Bottom = position.Y;
            Width = size.X;
            Height = size.Y;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Point"/>
        /// lies within the <see cref="Rectangle"/>.
        /// </summary>
        /// <param name="point">Point</param>
        /// <returns>True if contained</returns>
        public bool Contains(Point point) =>
            point.X >= Left && point.X <= Left + Width &&
            point.Y >= Bottom && point.Y <= Bottom + Height;

        /// <summary>
        /// Calculates a new <see cref="Rectangle"/> that is expanded
        /// by the specified amount compared to this rectangle.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public Rectangle Inflate(int left, int top, int right, int bottom)
        {
            var newLeft = Left - left;
            var newRight = Right + right;
            var newTop = Top + top;
            var newBottom = Bottom - bottom;
            return FromEdges(newLeft, newTop, newRight, newBottom);
        }

        /// <summary>
        /// Initializes a new <see cref="Rectangle"/> that
        /// uses the specified points as two corner points.
        /// </summary>
        /// <param name="p1">Point 1</param>
        /// <param name="p2">Point 2</param>
        /// <returns>Rectangle</returns>
        public static Rectangle FromCornerPoints(Point p1, Point p2)
        {
            return new Rectangle(
                Math.Min(p1.X, p2.X),
                Math.Min(p1.Y, p2.Y),
                Math.Abs(p1.X - p2.X),
                Math.Abs(p1.Y - p2.Y));
        }

        /// <summary>
        /// Initializes a new <see cref="Rectangle"/> with the specified edges.
        /// </summary>
        /// <param name="left">Left</param>
        /// <param name="top">Top</param>
        /// <param name="right">Right</param>
        /// <param name="bottom">Bottom</param>
        /// <returns>Rectangle</returns>
        public static Rectangle FromEdges(int left, int top, int right, int bottom)
        {
            if (right < left || top < bottom)
                throw new ArgumentOutOfRangeException();

            return new Rectangle(left, bottom, right - left, top - bottom);
        }

        /// <summary>
        /// Converts a <see cref="Point"/> to a <see cref="Rectangle"/>
        /// with zero size.
        /// </summary>
        /// <param name="p">Point</param>
        public static implicit operator Rectangle(Point p) => new Rectangle(p.X, p.Y, 0, 0);

        /// <summary>
        /// Returns a string representation of the <see cref="Point"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{Left}, {Bottom}, {Width}, {Height}]";

        /// <inheritDoc/>
        public override bool Equals(object obj)
        {
            return obj is Rectangle &&
                ((Rectangle)obj).Left == Left &&
                ((Rectangle)obj).Bottom == Bottom &&
                ((Rectangle)obj).Width == Width &&
                ((Rectangle)obj).Height == Height;
        }

        /// <inheritDoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritDoc/>
        public IEnumerator<Point> GetEnumerator()
        {
            for (var y = Bottom; y <= Top; y++)
                for (var x = Left; x <= Right; x++)
                    yield return new Point(x, y);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    class RectangleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Rectangle) || objectType == typeof(Rectangle?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                var s = reader.Value.ToString();
                var match = Regex.Match(s, @"^\[\s*([+-]?\d+)\s*,\s*([+-]?\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\]$");

                if (!match.Success)
                    throw new JsonSerializationException($"Failed to parse Rectangle from string '{s}'");

                return new Rectangle(
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value),
                    int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value));
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                JToken o = null;
                try
                {
                    o = JToken.ReadFrom(reader);

                    return new Rectangle(
                        o["X"].Value<int>(),
                        o["Y"].Value<int>(),
                        o["Width"].Value<int>(),
                        o["Height"].Value<int>());
                }
                catch
                {
                    throw new JsonSerializationException($"Failed to parse Rectangle from object '{o}'");
                }
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing Rectangle");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var rect = (Rectangle)value;
                writer.WriteValue($"[{rect.Left}, {rect.Bottom}, {rect.Width}, {rect.Height}]");
            }
        }
    }
}
