using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.RenderSettingss
{
	public struct SHCoefficientsBaked : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Sh0 = reader.ReadSingle();
			Sh1 = reader.ReadSingle();
			Sh2 = reader.ReadSingle();
			Sh3 = reader.ReadSingle();
			Sh4 = reader.ReadSingle();
			Sh5 = reader.ReadSingle();
			Sh6 = reader.ReadSingle();
			Sh7 = reader.ReadSingle();
			Sh8 = reader.ReadSingle();
			Sh9 = reader.ReadSingle();
			Sh10 = reader.ReadSingle();
			Sh11 = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("sh0", Sh0);
			node.Add("sh1", Sh1);
			node.Add("sh2", Sh2);
			node.Add("sh3", Sh3);
			node.Add("sh4", Sh4);
			node.Add("sh5", Sh5);
			node.Add("sh6", Sh6);
			node.Add("sh7", Sh7);
			node.Add("sh8", Sh8);
			node.Add("sh9", Sh9);
			node.Add("sh10", Sh10);
			node.Add("sh11", Sh11);
			return node;
		}

		public float Sh0 { get; private set; }
		public float Sh1 { get; private set; }
		public float Sh2 { get; private set; }
		public float Sh3 { get; private set; }
		public float Sh4 { get; private set; }
		public float Sh5 { get; private set; }
		public float Sh6 { get; private set; }
		public float Sh7 { get; private set; }
		public float Sh8 { get; private set; }
		public float Sh9 { get; private set; }
		public float Sh10 { get; private set; }
		public float Sh11 { get; private set; }
	}
}
