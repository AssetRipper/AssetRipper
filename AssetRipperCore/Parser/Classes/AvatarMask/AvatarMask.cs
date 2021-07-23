using AssetRipper.Converters.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using AssetRipper.YAML.Extensions;

namespace AssetRipper.Parser.Classes.AvatarMask
{
	public sealed class AvatarMask : NamedObject
	{
		public AvatarMask(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Mask = reader.ReadUInt32Array();
			Elements = reader.ReadAssetArray<TransformMaskElement>();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(MaskName, Mask.ExportYAML(true));
			node.Add(ElementsName, Elements.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "mask";

		public uint[] Mask { get; set; }
		public TransformMaskElement[] Elements { get; set; }

		public const string MaskName = "m_Mask";
		public const string ElementsName = "m_Elements";
	}
}
