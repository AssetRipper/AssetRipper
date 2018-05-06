using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.RenderSettingss
{
	/// <summary>
	/// LightProbeCoefficients and SH9Coefficients previously
	/// </summary>
	public struct SphericalHarmonicsL2 : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Not 5.0.0b1
		/// </summary>
		public static bool IsRead25(Version version)
		{
#warning unknown
			return !version.IsEqual(5, 0, 0, VersionType.Beta, 1);
		}

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
			Sh12 = stream.ReadSingle();
			Sh13 = stream.ReadSingle();
			Sh14 = stream.ReadSingle();
			Sh15 = stream.ReadSingle();
			Sh16 = stream.ReadSingle();
			Sh17 = stream.ReadSingle();
			Sh18 = stream.ReadSingle();
			Sh19 = stream.ReadSingle();
			Sh20 = stream.ReadSingle();
			Sh21 = stream.ReadSingle();
			Sh22 = stream.ReadSingle();
			Sh23 = stream.ReadSingle();
			Sh24 = stream.ReadSingle();
			if (IsRead25(stream.Version))
			{
				Sh25 = stream.ReadSingle();
				Sh26 = stream.ReadSingle();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
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
