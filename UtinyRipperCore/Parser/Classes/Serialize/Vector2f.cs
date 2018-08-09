using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public struct Vector2f : IScriptStructure
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

		public Vector2f(Vector2f copy):
			this(copy.X, copy.Y)
		{
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

		public IScriptStructure CreateCopy()
		{
			return new Vector2f(this);
		}

		public void Read(AssetStream stream)
		{
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
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
			node.Add("x", X);
			node.Add("y", Y);
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

		public override string ToString()
		{
			return $"[{X.ToString("0.00")}, {Y.ToString("0.00")}]";
		}

		public static Vector2f One { get; } = new Vector2f(1.0f, 1.0f);

		public IScriptStructure Base => null;
		public string Namespace => ScriptType.UnityEngineName;
		public string Name => ScriptType.Vector2Name;

		public float X { get; private set; }
		public float Y { get; private set; }
	}
}
