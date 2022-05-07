using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.AvatarMask
{
	public sealed class AvatarMask : NamedObject
	{
		public AvatarMask(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Mask = reader.ReadUInt32Array();
			Elements = reader.ReadAssetArray<TransformMaskElement>();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(MaskName, Mask.ExportYaml(true));
			node.Add(ElementsName, Elements.ExportYaml(container));
			return node;
		}

		public override string ExportExtension => "mask";

		public uint[] Mask { get; set; }
		public TransformMaskElement[] Elements { get; set; }

		public const string MaskName = "m_Mask";
		public const string ElementsName = "m_Elements";
	}
}
