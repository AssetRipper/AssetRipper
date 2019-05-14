using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct Vector4f : ISerializableStructure
	{
		public Vector4f(float value) :
			this(value, value, value, value)
		{
		}

		public Vector4f(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public ISerializableStructure CreateDuplicate()
		{
			return new Vector4f();
		}

		public void Read(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Z = reader.ReadSingle();
			W = reader.ReadSingle();
		}

		public void Read3(AssetReader reader)
		{
			X = reader.ReadSingle();
			Y = reader.ReadSingle();
			Z = reader.ReadSingle();
		}

		public void Write(BinaryWriter stream)
		{
			stream.Write(X);
			stream.Write(Y);
			stream.Write(Z);
			stream.Write(W);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode(MappingStyle.Flow);
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("z", Z);
			node.Add("w", W);
			return node;
		}

		public YAMLNode ExportYAML3(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode(MappingStyle.Flow);
			node.Add("x", X);
			node.Add("y", Y);
			node.Add("z", Z);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public override string ToString()
		{
			return $"[{X:0.00}, {Y:0.00}, {Z:0.00}, {W:0.00}]";
		}

		public float X { get; private set; }
		public float Y { get; private set; }
		public float Z { get; private set; }
		public float W { get; private set; }
	}
}
