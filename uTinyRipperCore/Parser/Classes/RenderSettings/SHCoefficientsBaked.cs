using uTinyRipper.Converters;
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
			node.Add(Sh0Name, Sh0);
			node.Add(Sh1Name, Sh1);
			node.Add(Sh2Name, Sh2);
			node.Add(Sh3Name, Sh3);
			node.Add(Sh4Name, Sh4);
			node.Add(Sh5Name, Sh5);
			node.Add(Sh6Name, Sh6);
			node.Add(Sh7Name, Sh7);
			node.Add(Sh8Name, Sh8);
			node.Add(Sh9Name, Sh9);
			node.Add(Sh10Name, Sh10);
			node.Add(Sh11Name, Sh11);
			return node;
		}

		public float Sh0 { get; set; }
		public float Sh1 { get; set; }
		public float Sh2 { get; set; }
		public float Sh3 { get; set; }
		public float Sh4 { get; set; }
		public float Sh5 { get; set; }
		public float Sh6 { get; set; }
		public float Sh7 { get; set; }
		public float Sh8 { get; set; }
		public float Sh9 { get; set; }
		public float Sh10 { get; set; }
		public float Sh11 { get; set; }

		public const string Sh0Name = "sh0";
		public const string Sh1Name = "sh1";
		public const string Sh2Name = "sh2";
		public const string Sh3Name = "sh3";
		public const string Sh4Name = "sh4";
		public const string Sh5Name = "sh5";
		public const string Sh6Name = "sh6";
		public const string Sh7Name = "sh7";
		public const string Sh8Name = "sh8";
		public const string Sh9Name = "sh9";
		public const string Sh10Name = "sh10";
		public const string Sh11Name = "sh11";
	}
}
