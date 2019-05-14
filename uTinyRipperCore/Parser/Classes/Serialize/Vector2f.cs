using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Vector2f : ISerializableStructure
	{
		public Vector2f(float value) :
			this(value, value)
		{
		}

		public Vector2f(float x, float y)
		{
			X = x;
			Y = y;
		}

		public static Vector2f operator -(Vector2f left)
		{
			return new Vector2f(-left.X, -left.Y);
		}

		public static Vector2f operator -(Vector2f left, Vector2f right)
		{
			return new Vector2f(left.X - right.X, left.Y - right.Y);
		}

		public static Vector2f operator +(Vector2f left, Vector2f right)
		{
			return new Vector2f(left.X + right.X, left.Y + right.Y);
		}

		public static Vector2f operator *(Vector2f left, float right)
		{
			return new Vector2f(left.X * right, left.Y * right);
		}

		public static Vector2f operator /(Vector2f left, float right)
		{
			return new Vector2f(left.X / right, left.Y / right);
		}
		
		public static bool operator ==(Vector2f left, Vector2f right)
		{
			return left.X == right.X && left.Y == right.Y;
		}

		public static bool operator !=(Vector2f left, Vector2f right)
		{
			return left.X != right.X || left.Y != right.Y;
		}

		/// <summary>
		/// Angle increase when 2nd line is moving in clockwise direction
		/// </summary>
		/// <returns>Angle in degrees</returns>
		public static float AngleFrom3Points(Vector2f point1, Vector2f point2, Vector2f point3)
		{
			Vector2f transformedP1 = new Vector2f(point1.X - point2.X, point1.Y - point2.Y);
			Vector2f transformedP2 = new Vector2f(point3.X - point2.X, point3.Y - point2.Y);

			double angleToP1 = Math.Atan2(transformedP1.Y, transformedP1.X);
			double angleToP2 = Math.Atan2(transformedP2.Y, transformedP2.X);

			double angle = angleToP1 - angleToP2;
			if (angle < 0)
			{
				angle += (2 * Math.PI);
			}
			return (float)(360.0 * angle / (2.0 * Math.PI));
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new Vector2f();
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write(X);
			stream.Write(Y);
		}
		
		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add(XName, X);
			node.Add(YName, Y);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public float GetMember(int index)
		{
			if (index == 0)
			{
				return X;
			}
			if (index == 1)
			{
				return Y;
			}
			throw new ArgumentException($"Invalid index {index}", nameof(index));
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != typeof(Vector2f))
			{
				return false;
			}
			return this == (Vector2f)obj;
		}

		public override int GetHashCode()
		{
			int hash = 61;
			unchecked
			{
				hash = hash + 977 * X.GetHashCode();
				hash = hash * 73 + Y.GetHashCode();
			}
			return hash;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0:0.00}, {1:0.00}]", X, Y);
		}

		public static Vector2f One { get; } = new Vector2f(1.0f, 1.0f);

		public float X { get; private set; }
		public float Y { get; private set; }

		public const string XName = "x";
		public const string YName = "y";
	}
}
