using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Game.Assembly;

namespace uTinyRipper.Classes
{
	public struct RectOffset : IAsset, ISerializableStructure
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

		public void Write(AssetWriter writer)
		{
			writer.Write(Left);
			writer.Write(Right);
			writer.Write(Top);
			writer.Write(Bottom);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(LeftName, Left);
			node.Add(RightName, Right);
			node.Add(TopName, Top);
			node.Add(BottomName, Bottom);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			yield break;
		}

		public int Left { get; private set; }
		public int Right { get; private set; }
		public int Top { get; private set; }
		public int Bottom { get; private set; }

		public const string LeftName = "m_Left";
		public const string RightName = "m_Right";
		public const string TopName = "m_Top";
		public const string BottomName = "m_Bottom";
	}
}
