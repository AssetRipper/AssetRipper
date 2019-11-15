using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public struct RectOffset : IAsset
	{
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
			RectOffsetLayout layout = container.ExportLayout.Serialized.RectOffset;
			node.Add(layout.LeftName, Left);
			node.Add(layout.RightName, Right);
			node.Add(layout.TopName, Top);
			node.Add(layout.BottomName, Bottom);
			return node;
		}

		public int Left { get; set; }
		public int Right { get; set; }
		public int Top { get; set; }
		public int Bottom { get; set; }
	}
}
