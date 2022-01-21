using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Flare
{
	public sealed class Flare : NamedObject
	{
		public Flare(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			FlareTexture.Read(reader);
			TextureLayout = (TextureLayout)reader.ReadInt32();
			Elements = reader.ReadAssetArray<FlareElement>();
			UseFog = reader.ReadBoolean();
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(FlareTexture, FlareTextureName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(1);
			node.Add(FlareTextureName, FlareTexture.ExportYAML(container));
			node.Add(TextureLayoutName, (int)TextureLayout);
			node.Add(ElementseName, Elements.ExportYAML(container));
			node.Add(UseFogName, UseFog);
			return node;
		}

		public TextureLayout TextureLayout { get; set; }
		public FlareElement[] Elements { get; set; }
		public bool UseFog { get; set; }

		public const string FlareTextureName = "m_FlareTexture";
		public const string TextureLayoutName = "m_TextureLayout";
		public const string ElementseName = "m_Elements";
		public const string UseFogName = "m_UseFog";

		public PPtr<Texture> FlareTexture = new();
	}
}
