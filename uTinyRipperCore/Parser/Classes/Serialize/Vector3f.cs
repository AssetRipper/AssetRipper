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
	public struct Vector3f : ISerializableStructure
	{
		public Vector3f(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new Vector3f();
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Z = reader.ReadSingle();
		}

		public void Read2(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
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
			node.Add(XName, X);
			node.Add(YName, Y);
			node.Add(ZName, Z);
			return node;
		}

		public YAMLNode ExportYAML2(IExportContainer container)
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
			return string.Format(CultureInfo.InvariantCulture, "[{0:0.00}, {1:0.00}, {2:0.00}]", X, Y, Z);
		}

		public static Vector3f One => new Vector3f(1.0f, 1.0f, 1.0f);

		public static Vector3f DefaultWeight => new Vector3f(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f);

		public float X { get; private set; }
		public float Y { get; private set; }
		public float Z { get; private set; }

		public const string XName = "x";
		public const string YName = "y";
		public const string ZName = "z";
	}
}
