using System.Collections.Generic;
using uTinyRipper.Assembly;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public struct RectOffset : ISerializableStructure
	{
		public ISerializableStructure CreateDuplicate()
		{
			return new RectOffset();
		}

		public void Read(AssetReader reader)
		{
			Left = reader.ReadInt32();
			Right = reader.ReadInt32();
			Top = reader.ReadInt32();
			Bottom = reader.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Left", Left);
			node.Add("m_Right", Right);
			node.Add("m_Top", Top);
			node.Add("m_Bottom", Bottom);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public int Left { get; private set; }
		public int Right { get; private set; }
		public int Top { get; private set; }
		public int Bottom { get; private set; }
	}
}
