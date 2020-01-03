using uTinyRipper.Converters;
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
		public static bool Has25(Version version)
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
			if (Has25(reader.Version))
			{
				Sh25 = reader.ReadSingle();
				Sh26 = reader.ReadSingle();
			}
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
			node.Add(Sh12Name, Sh12);
			node.Add(Sh13Name, Sh13);
			node.Add(Sh14Name, Sh14);
			node.Add(Sh15Name, Sh15);
			node.Add(Sh16Name, Sh16);
			node.Add(Sh17Name, Sh17);
			node.Add(Sh18Name, Sh18);
			node.Add(Sh19Name, Sh19);
			node.Add(Sh20Name, Sh20);
			node.Add(Sh21Name, Sh21);
			node.Add(Sh22Name, Sh22);
			node.Add(Sh23Name, Sh23);
			node.Add(Sh24Name, Sh24);
			node.Add(Sh25Name, Sh25);
			node.Add(Sh26Name, Sh26);
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
		public float Sh12 { get; set; }
		public float Sh13 { get; set; }
		public float Sh14 { get; set; }
		public float Sh15 { get; set; }
		public float Sh16 { get; set; }
		public float Sh17 { get; set; }
		public float Sh18 { get; set; }
		public float Sh19 { get; set; }
		public float Sh20 { get; set; }
		public float Sh21 { get; set; }
		public float Sh22 { get; set; }
		public float Sh23 { get; set; }
		public float Sh24 { get; set; }
		public float Sh25 { get; set; }
		public float Sh26 { get; set; }

		public const string Sh0Name = "sh[ 0]";
		public const string Sh1Name = "sh[ 1]";
		public const string Sh2Name = "sh[ 2]";
		public const string Sh3Name = "sh[ 3]";
		public const string Sh4Name = "sh[ 4]";
		public const string Sh5Name = "sh[ 5]";
		public const string Sh6Name = "sh[ 6]";
		public const string Sh7Name = "sh[ 7]";
		public const string Sh8Name = "sh[ 8]";
		public const string Sh9Name = "sh[ 9]";
		public const string Sh10Name = "sh[10]";
		public const string Sh11Name = "sh[11]";
		public const string Sh12Name = "sh[12]";
		public const string Sh13Name = "sh[13]";
		public const string Sh14Name = "sh[14]";
		public const string Sh15Name = "sh[15]";
		public const string Sh16Name = "sh[16]";
		public const string Sh17Name = "sh[17]";
		public const string Sh18Name = "sh[18]";
		public const string Sh19Name = "sh[19]";
		public const string Sh20Name = "sh[20]";
		public const string Sh21Name = "sh[21]";
		public const string Sh22Name = "sh[22]";
		public const string Sh23Name = "sh[23]";
		public const string Sh24Name = "sh[24]";
		public const string Sh25Name = "sh[25]";
		public const string Sh26Name = "sh[26]";
	}
}
