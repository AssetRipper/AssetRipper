using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.RenderSettingss
{
	/// <summary>
	/// LightProbeCoefficients and SH9Coefficients previously
	/// </summary>
	public struct SphericalHarmonicsL2 : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Not 5.0.0b
		/// </summary>
		public static bool IsRead25(Version version)
		{
			// unknown version
			return !version.IsEqual(5, 0, 0, VersionType.Beta);
		}

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
			Sh12 = reader.ReadSingle();
			Sh13 = reader.ReadSingle();
			Sh14 = reader.ReadSingle();
			Sh15 = reader.ReadSingle();
			Sh16 = reader.ReadSingle();
			Sh17 = reader.ReadSingle();
			Sh18 = reader.ReadSingle();
			Sh19 = reader.ReadSingle();
			Sh20 = reader.ReadSingle();
			Sh21 = reader.ReadSingle();
			Sh22 = reader.ReadSingle();
			Sh23 = reader.ReadSingle();
			Sh24 = reader.ReadSingle();
			if (IsRead25(reader.Version))
			{
				Sh25 = reader.ReadSingle();
				Sh26 = reader.ReadSingle();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("sh[ 0]", Sh0);
			node.Add("sh[ 1]", Sh1);
			node.Add("sh[ 2]", Sh2);
			node.Add("sh[ 3]", Sh3);
			node.Add("sh[ 4]", Sh4);
			node.Add("sh[ 5]", Sh5);
			node.Add("sh[ 6]", Sh6);
			node.Add("sh[ 7]", Sh7);
			node.Add("sh[ 8]", Sh8);
			node.Add("sh[ 9]", Sh9);
			node.Add("sh[10]", Sh10);
			node.Add("sh[11]", Sh11);
			node.Add("sh[12]", Sh12);
			node.Add("sh[13]", Sh13);
			node.Add("sh[14]", Sh14);
			node.Add("sh[15]", Sh15);
			node.Add("sh[16]", Sh16);
			node.Add("sh[17]", Sh17);
			node.Add("sh[18]", Sh18);
			node.Add("sh[19]", Sh19);
			node.Add("sh[20]", Sh20);
			node.Add("sh[21]", Sh21);
			node.Add("sh[22]", Sh22);
			node.Add("sh[23]", Sh23);
			node.Add("sh[24]", Sh24);
			node.Add("sh[25]", Sh25);
			node.Add("sh[26]", Sh26);
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
		public float Sh12 { get; private set; }
		public float Sh13 { get; private set; }
		public float Sh14 { get; private set; }
		public float Sh15 { get; private set; }
		public float Sh16 { get; private set; }
		public float Sh17 { get; private set; }
		public float Sh18 { get; private set; }
		public float Sh19 { get; private set; }
		public float Sh20 { get; private set; }
		public float Sh21 { get; private set; }
		public float Sh22 { get; private set; }
		public float Sh23 { get; private set; }
		public float Sh24 { get; private set; }
		public float Sh25 { get; private set; }
		public float Sh26 { get; private set; }
	}
}
