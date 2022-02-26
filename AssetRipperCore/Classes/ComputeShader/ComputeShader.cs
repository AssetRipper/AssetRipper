using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ComputeShader
{
	public sealed class ComputeShader : NamedObject
	{
		public ComputeShader(AssetInfo assetInfo) : base(assetInfo) { }

		public override string ExportExtension => AssetExtension;

		public static int ToSerializedVersion(UnityVersion version)
		{
			return 1;
		}

		public override void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Variants = reader.ReadAssetArray<ComputeShaderVariant>();
			reader.AlignStream();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add("m_Name", Name);
			node.Add("variants", Variants.ExportYAML(container));
			return node;
		}

		public ComputeShaderVariant[] Variants { get; set; }
	}
}
