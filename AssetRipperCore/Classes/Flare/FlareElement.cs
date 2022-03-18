using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Flare
{
	public sealed class FlareElement : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ImageIndex = reader.ReadInt32();
			Position = reader.ReadSingle();
			Size = reader.ReadSingle();
			Color.Read(reader);
			UseLightColor = reader.ReadBoolean();
			Rotate = reader.ReadBoolean();
			Zoom = reader.ReadBoolean();
			Fade = reader.ReadBoolean();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ImageIndexName, ImageIndex);
			node.Add(PositionName, Position);
			node.Add(SizeName, Size);
			node.Add(ColorName, Color.ExportYAML(container));
			node.Add(UseLightColorName, UseLightColor);
			node.Add(RotateName, Rotate);
			node.Add(ZoomName, Zoom);
			node.Add(FadeName, Fade);
			return node;
		}


		public int ImageIndex { get; set; }
		public float Position { get; set; }
		public float Size { get; set; }
		public ColorRGBAf Color = new();
		public bool UseLightColor { get; set; }
		public bool Rotate { get; set; }
		public bool Zoom { get; set; }
		public bool Fade { get; set; }

		public const string ImageIndexName = "m_ImageIndex";
		public const string PositionName = "m_Position";
		public const string SizeName = "m_Size";
		public const string ColorName = "m_Color";
		public const string UseLightColorName = "m_UseLightColor";
		public const string RotateName = "m_Rotate";
		public const string ZoomName = "m_Zoom";
		public const string FadeName = "m_Fade";
	}
}
