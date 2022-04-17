using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Flare
{
	public sealed class Halo : Behaviour
	{
		public Halo(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Color.Read(reader);
			Size = reader.ReadSingle();
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(1);
			node.Add("m_Color", Color.ExportYaml(container));
			node.Add("m_Brightness", Size);
			return node;
		}

		public ColorRGBA32 Color = new();
		public float Size { get; set; }
	}
}
