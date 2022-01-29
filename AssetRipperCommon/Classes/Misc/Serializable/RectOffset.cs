using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc.Serializable
{
	public sealed class RectOffset : IAsset
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
			node.Add(LeftName, Left);
			node.Add(RightName, Right);
			node.Add(TopName, Top);
			node.Add(BottomName, Bottom);
			return node;
		}

		public int Left { get; set; }
		public int Right { get; set; }
		public int Top { get; set; }
		public int Bottom { get; set; }

		public const string LeftName = "m_Left";
		public const string RightName = "m_Right";
		public const string TopName = "m_Top";
		public const string BottomName = "m_Bottom";
	}
}
