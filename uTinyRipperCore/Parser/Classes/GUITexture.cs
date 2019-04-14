using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class GUITexture : GUIElement
	{
		public GUITexture(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Texture.Read(reader);
			Color.Read(reader);
			PixelInset.Read(reader);
			LeftBorder = reader.ReadInt32();
			RightBorder = reader.ReadInt32();
			TopBorder = reader.ReadInt32();
			BottomBorder = reader.ReadInt32();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return Texture.FetchDependency(file, isLog, ToLogString, TextureName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(TextureName, Texture.ExportYAML(container));
			node.Add(ColorName, Color.ExportYAML(container));
			node.Add(PixelInsetName, PixelInset.ExportYAML(container));
			node.Add(LeftBorderName, LeftBorder);
			node.Add(RightBorderName, RightBorder);
			node.Add(TopBorderName, TopBorder);
			node.Add(BottomBorderName, BottomBorder);
			return node;
		}

		public int LeftBorder { get; private set; }
		public int RightBorder { get; private set; }
		public int TopBorder { get; private set; }
		public int BottomBorder { get; private set; }

		public const string TextureName = "m_Texture";
		public const string ColorName = "m_Color";
		public const string PixelInsetName = "m_PixelInset";
		public const string LeftBorderName = "m_LeftBorder";
		public const string RightBorderName = "m_RightBorder";
		public const string TopBorderName = "m_TopBorder";
		public const string BottomBorderName = "m_BottomBorder";

		public PPtr<Texture> Texture;
		public ColorRGBAf Color;
		public Rectf PixelInset;
	}
}
