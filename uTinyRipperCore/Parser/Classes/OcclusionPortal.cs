using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class OcclusionPortal : Component
	{
		public OcclusionPortal(AssetInfo assetInfo):
			base(assetInfo)
		{
		}
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Open = reader.ReadBoolean();
			reader.AlignStream();
			
			Center.Read(reader);
			Size.Read(reader);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(OpenName, Open);
			node.Add(CenterName, Center.ExportYAML(container));
			node.Add(SizeName, Size.ExportYAML(container));
			return node;
		}

		public bool Open { get; set; }

		public const string OpenName = "m_Open";
		public const string CenterName = "m_Center";
		public const string SizeName = "m_Size";

		public Vector3f Center;
		public Vector3f Size;
	}
}
