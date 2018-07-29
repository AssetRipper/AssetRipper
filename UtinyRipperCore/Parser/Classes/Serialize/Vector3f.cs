using System;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct Vector3f : IAssetReadable, IYAMLExportable
	{
		public Vector3f(float value):
			this(value, value, value)
		{
		}

		public Vector3f(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}
		
		public void Read(AssetStream stream)
		{
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
			Z = stream.ReadSingle();
		}

		public void Read2(AssetStream stream)
		{
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write(X);
			stream.Write(Y);
			stream.Write(Z);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("z", Z);
			return node;
		}

		public YAMLNode ExportYAML2(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add("x", X);
			node.Add("y", Y);
			return node;
		}

		public Vector2f ToVector2()
		{
			return new Vector2f(X, Y);
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
			if (index == 2)
			{
				return Z;
			}
			throw new ArgumentException($"Invalid index {index}", nameof(index));
		}

		public override string ToString()
		{
			return $"[{X:0.00}, {Y:0.00}, {Z:0.00}]";
		}

		public static Vector3f One => new Vector3f(1.0f, 1.0f, 1.0f);

		public float X { get; private set; }
		public float Y { get; private set; }
		public float Z { get; private set; }
	}
}
