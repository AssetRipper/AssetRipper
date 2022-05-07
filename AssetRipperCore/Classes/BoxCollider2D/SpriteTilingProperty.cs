using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.BoxCollider2D
{
	public sealed class SpriteTilingProperty : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Border.Read(reader);
			Pivot.Read(reader);
			OldSize.Read(reader);
			NewSize.Read(reader);
			AdaptiveTilingThreshold = reader.ReadSingle();
			DrawMode = reader.ReadInt32();
			AdaptiveTiling = reader.ReadBoolean();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(BorderName, Border.ExportYaml(container));
			node.Add(PivotName, Pivot.ExportYaml(container));
			node.Add(OldSizeName, OldSize.ExportYaml(container));
			node.Add(NewSizeName, NewSize.ExportYaml(container));
			node.Add(AdaptiveTilingThresholdName, AdaptiveTilingThreshold);
			node.Add(DrawModeName, DrawMode);
			node.Add(AdaptiveTilingName, AdaptiveTiling);
			return node;
		}

		public float AdaptiveTilingThreshold { get; set; }
		public int DrawMode { get; set; }
		public bool AdaptiveTiling { get; set; }

		public const string BorderName = "border";
		public const string PivotName = "pivot";
		public const string OldSizeName = "oldSize";
		public const string NewSizeName = "newSize";
		public const string AdaptiveTilingThresholdName = "adaptiveTilingThreshold";
		public const string DrawModeName = "drawMode";
		public const string AdaptiveTilingName = "adaptiveTiling";

		public Vector4f Border = new();
		public Vector2f Pivot = new();
		public Vector2f OldSize = new();
		public Vector2f NewSize = new();
	}
}
