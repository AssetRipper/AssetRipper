using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public sealed class GUITexture : GUIElement
	{
		public GUITexture(AssetInfo assetInfo) : base(assetInfo) { }

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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Texture, TextureName);
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

		public int LeftBorder { get; set; }
		public int RightBorder { get; set; }
		public int TopBorder { get; set; }
		public int BottomBorder { get; set; }

		public const string TextureName = "m_Texture";
		public const string ColorName = "m_Color";
		public const string PixelInsetName = "m_PixelInset";
		public const string LeftBorderName = "m_LeftBorder";
		public const string RightBorderName = "m_RightBorder";
		public const string TopBorderName = "m_TopBorder";
		public const string BottomBorderName = "m_BottomBorder";

		public PPtr<Texture> Texture = new();
		public ColorRGBAf Color = new();
		public Rectf PixelInset = new();
	}
}
