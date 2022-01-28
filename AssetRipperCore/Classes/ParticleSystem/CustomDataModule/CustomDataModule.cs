using AssetRipper.Core.Classes.ParticleSystem.Curve;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.ParticleSystem.CustomDataModule
{
	public sealed class CustomDataModule : ParticleSystemModule
	{
		public CustomDataModule() { }

		public CustomDataModule(bool _)
		{
			VectorComponentCount0 = 4;
			Color0 = new MinMaxGradient.MinMaxGradient(true);
			Vector0_0 = new MinMaxCurve(0.0f);
			Vector0_1 = new MinMaxCurve(0.0f);
			Vector0_2 = new MinMaxCurve(0.0f);
			Vector0_3 = new MinMaxCurve(0.0f);

			VectorComponentCount0 = 4;
			Color1 = new MinMaxGradient.MinMaxGradient(true);
			Vector1_0 = new MinMaxCurve(0.0f);
			Vector1_1 = new MinMaxCurve(0.0f);
			Vector1_2 = new MinMaxCurve(0.0f);
			Vector1_3 = new MinMaxCurve(0.0f);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Mode0 = (ParticleSystemCustomDataMode)reader.ReadInt32();
			VectorComponentCount0 = reader.ReadInt32();
			Color0.Read(reader);

			Vector0_0.Read(reader);
			Vector0_1.Read(reader);
			Vector0_2.Read(reader);
			Vector0_3.Read(reader);

			Mode1 = (ParticleSystemCustomDataMode)reader.ReadInt32();
			VectorComponentCount1 = reader.ReadInt32();
			Color1.Read(reader);

			Vector1_0.Read(reader);
			Vector1_1.Read(reader);
			Vector1_2.Read(reader);
			Vector1_3.Read(reader);
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(Mode0Name, (int)Mode0);
			node.Add(VectorComponentCount0Name, VectorComponentCount0);
			node.Add(Color0Name, Color0.ExportYAML(container));
			node.Add(Vector00Name, Vector0_0.ExportYAML(container));
			node.Add(Vector01Name, Vector0_1.ExportYAML(container));
			node.Add(Vector02Name, Vector0_2.ExportYAML(container));
			node.Add(Vector03Name, Vector0_3.ExportYAML(container));
			node.Add(Mode1Name, (int)Mode1);
			node.Add(VectorComponentCount1Name, VectorComponentCount1);
			node.Add(Color1Name, Color1.ExportYAML(container));
			node.Add(Vector10Name, Vector1_0.ExportYAML(container));
			node.Add(Vector11Name, Vector1_1.ExportYAML(container));
			node.Add(Vector12Name, Vector1_2.ExportYAML(container));
			node.Add(Vector13Name, Vector1_3.ExportYAML(container));
			return node;
		}

		public ParticleSystemCustomDataMode Mode0 { get; set; }
		public int VectorComponentCount0 { get; set; }

		public ParticleSystemCustomDataMode Mode1 { get; set; }
		public int VectorComponentCount1 { get; set; }

		public const string Mode0Name = "mode0";
		public const string VectorComponentCount0Name = "vectorComponentCount0";
		public const string Color0Name = "color0";
		public const string Vector00Name = "vector0_0";
		public const string Vector01Name = "vector0_1";
		public const string Vector02Name = "vector0_2";
		public const string Vector03Name = "vector0_3";
		public const string Mode1Name = "mode1";
		public const string VectorComponentCount1Name = "vectorComponentCount1";
		public const string Color1Name = "color1";
		public const string Vector10Name = "vector1_0";
		public const string Vector11Name = "vector1_1";
		public const string Vector12Name = "vector1_2";
		public const string Vector13Name = "vector1_3";

		public MinMaxGradient.MinMaxGradient Color0 = new();
		public MinMaxCurve Vector0_0 = new();
		public MinMaxCurve Vector0_1 = new();
		public MinMaxCurve Vector0_2 = new();
		public MinMaxCurve Vector0_3 = new();
		public MinMaxGradient.MinMaxGradient Color1 = new();
		public MinMaxCurve Vector1_0 = new();
		public MinMaxCurve Vector1_1 = new();
		public MinMaxCurve Vector1_2 = new();
		public MinMaxCurve Vector1_3 = new();
	}
}
