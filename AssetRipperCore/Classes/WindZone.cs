using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public class WindZone : Behaviour
	{
		public WindZone(LayoutInfo layout) : base(layout) { }
		public WindZone(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			Mode = reader.ReadInt32();
			Radius = reader.ReadSingle();
			WindMain = reader.ReadSingle();
			WindTurbulence = reader.ReadSingle();
			WindPulseMagnitude = reader.ReadSingle();
			WindPulseFrequency = reader.ReadSingle();
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);
			writer.Write(Mode);
			writer.Write(Radius);
			writer.Write(WindMain);
			writer.Write(WindTurbulence);
			writer.Write(WindPulseMagnitude);
			writer.Write(WindPulseFrequency);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ModeName, Mode);
			node.Add(RadiusName, Radius);
			node.Add(WindMainName, WindMain);
			node.Add(WindTurbulenceName, WindTurbulence);
			node.Add(WindPulseMagnitudeName, WindPulseMagnitude);
			node.Add(WindPulseFrequencyName, WindPulseFrequency);
			return node;
		}

		public int Mode { get; set; }
		public float Radius { get; set; }
		public float WindMain { get; set; }
		public float WindTurbulence { get; set; }
		public float WindPulseMagnitude { get; set; }
		public float WindPulseFrequency { get; set; }

		public const string ModeName = "m_Mode";
		public const string RadiusName = "m_Radius";
		public const string WindMainName = "m_WindMain";
		public const string WindTurbulenceName = "m_WindTurbulence";
		public const string WindPulseMagnitudeName = "m_WindPulseMagnitude";
		public const string WindPulseFrequencyName = "m_WindPulseFrequency";
	}
}
