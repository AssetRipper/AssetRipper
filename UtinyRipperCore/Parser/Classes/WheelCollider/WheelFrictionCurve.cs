using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.WheelColliders
{
	public struct WheelFrictionCurve : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			ExtremumSlip = stream.ReadSingle();
			ExtremumValue = stream.ReadSingle();
			AsymptoteSlip = stream.ReadSingle();
			AsymptoteValue = stream.ReadSingle();
			Stiffness = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_ExtremumSlip", ExtremumSlip);
			node.Add("m_ExtremumValue", ExtremumValue);
			node.Add("m_AsymptoteSlip", AsymptoteSlip);
			node.Add("m_AsymptoteValue", AsymptoteValue);
			node.Add("m_Stiffness", Stiffness);
			return node;
		}

		public float ExtremumSlip { get; private set; }
		public float ExtremumValue { get; private set; }
		public float AsymptoteSlip { get; private set; }
		public float AsymptoteValue { get; private set; }
		public float Stiffness { get; private set; }
	}
}
