using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text.RegularExpressions;

namespace OrangeBugReloaded.Core
{
    /// <summary>
    /// Represents a point with integer X and Y values.
    /// It is assumed that the X axis points to the right
    /// and the Y axis points upwards.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    //[JsonConverter(typeof(PointConverter))]
    public struct Point
    {
        /// <summary>
        /// Gets the points that represent a direction.
        /// These are <see cref="North"/>, <see cref="East"/>,
        /// <see cref="South"/> and <see cref="West"/>.
        /// </summary>
        public static IEnumerable<Point> Directions => GetDirections();

        /// <summary>
        /// The <see cref="Point"/> [0, 0].
        /// </summary>
        public static Point Zero { get; } = new Point(0, 0);

        /// <summary>
        /// The <see cref="Point"/> [1, 1].
        /// </summary>
        public static Point One { get; } = new Point(1, 1);

        /// <summary>
        /// The <see cref="Point"/> [1, 0].
        /// </summary>
        public static Point East { get; } = new Point(1, 0);

        /// <summary>
        /// The <see cref="Point"/> [-1, 0].
        /// </summary>
        public static Point West { get; } = new Point(-1, 0);

        /// <summary>
        /// The <see cref="Point"/> [0, 1].
        /// </summary>
        public static Point North { get; } = new Point(0, 1);

        /// <summary>
        /// The <see cref="Point"/> [0, -1].
        /// </summary>
        public static Point South { get; } = new Point(0, -1);

        /// <summary>
        /// X coordinate.
        /// </summary>
        [JsonProperty]
        public int X { get; }

        /// <summary>
        /// Y coordinate.
        /// </summary>
        [JsonProperty]
        public int Y { get; }

        /// <summary>
        /// True if the point indicates a direction
        /// (<see cref="North"/>, <see cref="East"/>, <see cref="South"/> or <see cref="West"/>).
        /// </summary>
        public bool IsDirection => (Math.Abs(X) == 1 && Y == 0) || (Math.Abs(Y) == 1 && X == 0);

        /// <summary>
        /// Initializes a new <see cref="Point"/> with the specified coordinates.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        [DebuggerStepThrough]
        [JsonConstructor]
        public Point(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Restricts the point to the specified
        /// minimum and maximum values.
        /// </summary>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <returns>New Point</returns>
        public Point Clamp(Point min, Point max)
        {
            return new Point(
                Math.Min(max.X, Math.Max(min.X, X)),
                Math.Min(max.Y, Math.Max(min.Y, Y)));
        }

#pragma warning disable CS1591 // Fehledes XML-Kommentar für öffentlich sichtbaren Typ oder Element

        public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point p) => new Point(-p.X, -p.Y);
        public static Point operator -(Point a, Point b) => new Point(a.X - b.X, a.Y - b.Y);
        public static Point operator *(Point p, int scale) => scale * p;
        public static Point operator *(int scale, Point p) => new Point(scale * p.X, scale * p.Y);
        public static Point operator /(Point p, double div) => new Point((int)Math.Floor(p.X / div), (int)Math.Floor(p.Y / div));
        public static Point operator %(Point p, int div) => new Point(Mod(p.X, div), Mod(p.Y, div));
        public static bool operator ==(Point a, Point b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Point a, Point b) => a.X != b.X || a.Y != b.Y;

        public static Point Min(Point a, Point b) => new Point(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y));

        public static Point Max(Point a, Point b) => new Point(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

        public static Point Distance(Point a, Point b) => new Point(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

#pragma warning restore CS1591 // Fehledes XML-Kommentar für öffentlich sichtbaren Typ oder Element

        /// <summary>
        /// Throws an exception if the <see cref="Point"/> is not a direction,
        /// i.e. if <see cref="IsDirection"/> is false.
        /// </summary>
        /// <remarks>The point</remarks>
        public Point EnsureDirection()
        {
            if (!IsDirection)
                throw new ArgumentException("The specified point does not represent a direction");
            return this;
        }

        /// <summary>
        /// Returns a string representation of the <see cref="Point"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"[{X}, {Y}]";

        /// <inheritDoc/>
        public override bool Equals(object obj)
        {
            return obj is Point && ((Point)obj).X == X && ((Point)obj).Y == Y;
        }

        /// <inheritDoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Vector2 ToVector2() => new Vector2(X, Y);

        private static int Mod(int x, int m)
        {
            // http://stackoverflow.com/questions/1082917/mod-of-negative-number-is-melting-my-brain
            return (x % m + m) % m;
        }

        private static IEnumerable<Point> GetDirections()
        {
            yield return North;
            yield return East;
            yield return South;
            yield return West;
        }
    }

    public static class PointExtensions
    {
        public static Point ToOrangeBugPoint(this Vector2 v)
        {
            return new Point((int)Math.Floor(v.X), (int)Math.Ceiling(v.Y));
        }
    }

    class PointConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(Point) || objectType == typeof(Point?);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                var s = reader.Value.ToString();
                var match = Regex.Match(s, @"^\[\s*([+-]?\d+)\s*,\s*([+-]?\d+)\s*\]$");

                if (!match.Success)
                    throw new JsonSerializationException($"Failed to parse Point from string '{s}'");

                return new Point(
                    int.Parse(match.Groups[1].Value),
                    int.Parse(match.Groups[2].Value));
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                JToken o = null;
                try
                {
                    o = JToken.ReadFrom(reader);

                    return new Point(
                        o["X"].Value<int>(),
                        o["Y"].Value<int>());
                }
                catch
                {
                    throw new JsonSerializationException($"Failed to parse Point from object '{o}'");
                }
            }

            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing Point");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var p = (Point)value;
                writer.WriteValue($"[{p.X}, {p.Y}]");
            }
        }
    }
}
