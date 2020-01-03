using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.WheelColliders
{
	public struct WheelFrictionCurve : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ExtremumSlip = reader.ReadSingle();
			ExtremumValue = reader.ReadSingle();
			AsymptoteSlip = reader.ReadSingle();
			AsymptoteValue = reader.ReadSingle();
			Stiffness = reader.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ExtremumSlipName, ExtremumSlip);
			node.Add(ExtremumValueName, ExtremumValue);
			node.Add(AsymptoteSlipName, AsymptoteSlip);
			node.Add(AsymptoteValueName, AsymptoteValue);
			node.Add(StiffnessName, Stiffness);
			return node;
		}

		public float ExtremumSlip { get; set; }
		public float ExtremumValue { get; set; }
		public float AsymptoteSlip { get; set; }
		public float AsymptoteValue { get; set; }
		public float Stiffness { get; set; }

		public const string ExtremumSlipName = "m_ExtremumSlip";
		public const string ExtremumValueName = "m_ExtremumValue";
		public const string AsymptoteSlipName = "m_AsymptoteSlip";
		public const string AsymptoteValueName = "m_AsymptoteValue";
		public const string StiffnessName = "m_Stiffness";
	}
}
