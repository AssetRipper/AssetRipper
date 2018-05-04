using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public class CustomDataModule : ParticleSystemModule
	{
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadLabel(TransferInstructionFlags flags)
		{
			return !flags.IsSerializeGameRelease();
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Mode0 = stream.ReadInt32();
			VectorComponentCount0 = stream.ReadInt32();
			Color0.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				ColorLabel0 = stream.ReadStringAligned();
			}
			Vector0_0.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				VectorLabel0_0 = stream.ReadStringAligned();
			}
			Vector0_1.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				VectorLabel0_1 = stream.ReadStringAligned();
			}
			Vector0_2.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				VectorLabel0_2 = stream.ReadStringAligned();
			}
			Vector0_3.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				VectorLabel0_3 = stream.ReadStringAligned();
			}
			Mode1 = stream.ReadInt32();
			VectorComponentCount1 = stream.ReadInt32();
			Color1.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				ColorLabel1 = stream.ReadStringAligned();
			}
			Vector1_0.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				VectorLabel1_0 = stream.ReadStringAligned();
			}
			Vector1_1.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				VectorLabel1_1 = stream.ReadStringAligned();
			}
			Vector1_2.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				VectorLabel1_2 = stream.ReadStringAligned();
			}
			Vector1_3.Read(stream);
			if (IsReadLabel(stream.Flags))
			{
				VectorLabel1_3 = stream.ReadStringAligned();
			}
		}

		public override YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(exporter);
			node.Add("mode0", Mode0);
			node.Add("vectorComponentCount0", VectorComponentCount0);
			node.Add("color0", Color0.ExportYAML(exporter));
			node.Add("colorLabel0", IsReadLabel(exporter.Flags) ? ColorLabel0 : "Color");
			node.Add("vector0_0", Vector0_0.ExportYAML(exporter));
			node.Add("vectorLabel0_0", IsReadLabel(exporter.Flags) ? VectorLabel0_0 : "X");
			node.Add("vector0_1", Vector0_1.ExportYAML(exporter));
			node.Add("vectorLabel0_1", IsReadLabel(exporter.Flags) ? VectorLabel0_1 : "Y");
			node.Add("vector0_2", Vector0_2.ExportYAML(exporter));
			node.Add("vectorLabel0_2", IsReadLabel(exporter.Flags) ? VectorLabel0_2 : "Z");
			node.Add("vector0_3", Vector0_3.ExportYAML(exporter));
			node.Add("vectorLabel0_3", IsReadLabel(exporter.Flags) ? VectorLabel0_3 : "W");
			node.Add("mode1", Mode1);
			node.Add("vectorComponentCount1", VectorComponentCount1);
			node.Add("color1", Color1.ExportYAML(exporter));
			node.Add("colorLabel1", IsReadLabel(exporter.Flags) ? ColorLabel1 : "Color");
			node.Add("vector1_0", Vector1_0.ExportYAML(exporter));
			node.Add("vectorLabel1_0", IsReadLabel(exporter.Flags) ? VectorLabel1_0 : "X");
			node.Add("vector1_1", Vector1_1.ExportYAML(exporter));
			node.Add("vectorLabel1_1", IsReadLabel(exporter.Flags) ? VectorLabel1_1 : "Y");
			node.Add("vector1_2", Vector1_2.ExportYAML(exporter));
			node.Add("vectorLabel1_2", IsReadLabel(exporter.Flags) ? VectorLabel1_2 : "Z");
			node.Add("vector1_3", Vector1_3.ExportYAML(exporter));
			node.Add("vectorLabel1_3", IsReadLabel(exporter.Flags) ? VectorLabel1_3 : "W");
			return node;
		}

		public int Mode0 { get; private set; }
		public int VectorComponentCount0 { get; private set; }
		public string ColorLabel0 { get; private set; }
		public string VectorLabel0_0 { get; private set; }
		public string VectorLabel0_1 { get; private set; }
		public string VectorLabel0_2 { get; private set; }
		public string VectorLabel0_3 { get; private set; }
		public int Mode1 { get; private set; }
		public int VectorComponentCount1 { get; private set; }
		public string ColorLabel1 { get; private set; }
		public string VectorLabel1_0 { get; private set; }
		public string VectorLabel1_1 { get; private set; }
		public string VectorLabel1_2 { get; private set; }
		public string VectorLabel1_3 { get; private set; }

		public MinMaxGradient Color0;
		public MinMaxCurve Vector0_0;
		public MinMaxCurve Vector0_1;
		public MinMaxCurve Vector0_2;
		public MinMaxCurve Vector0_3;
		public MinMaxGradient Color1;
		public MinMaxCurve Vector1_0;
		public MinMaxCurve Vector1_1;
		public MinMaxCurve Vector1_2;
		public MinMaxCurve Vector1_3;
	}
}
