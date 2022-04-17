using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes
{
	public sealed class OcclusionPortal : Component, IOcclusionPortal
	{
		public OcclusionPortal(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Open = reader.ReadBoolean();
			reader.AlignStream();

			Center.Read(reader);
			Size.Read(reader);
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(OpenName, Open);
			node.Add(CenterName, Center.ExportYaml(container));
			node.Add(SizeName, Size.ExportYaml(container));
			return node;
		}

		public bool Open { get; set; }

		public const string OpenName = "m_Open";
		public const string CenterName = "m_Center";
		public const string SizeName = "m_Size";

		public Vector3f Center = new();
		public Vector3f Size = new();
	}
}
