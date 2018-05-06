using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.RenderSettingss
{
	public struct SHCoefficientsBaked : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			Sh0 = stream.ReadSingle();
			Sh1 = stream.ReadSingle();
			Sh2 = stream.ReadSingle();
			Sh3 = stream.ReadSingle();
			Sh4 = stream.ReadSingle();
			Sh5 = stream.ReadSingle();
			Sh6 = stream.ReadSingle();
			Sh7 = stream.ReadSingle();
			Sh8 = stream.ReadSingle();
			Sh9 = stream.ReadSingle();
			Sh10 = stream.ReadSingle();
			Sh11 = stream.ReadSingle();
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
