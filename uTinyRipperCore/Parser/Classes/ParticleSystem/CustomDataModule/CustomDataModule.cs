using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.ParticleSystems
{
	public sealed class CustomDataModule : ParticleSystemModule
	{
		public CustomDataModule()
		{
		}

		public CustomDataModule(bool _)
		{
			VectorComponentCount0 = 4;
			Color0 = new MinMaxGradient(true);
			Vector0_0 = new MinMaxCurve(0.0f);
			Vector0_1 = new MinMaxCurve(0.0f);
			Vector0_2 = new MinMaxCurve(0.0f);
			Vector0_3 = new MinMaxCurve(0.0f);
#if UNIVERSAL
			ColorLabel0 = "Color";
			VectorLabel0_0 = "X";
			VectorLabel0_1 = "Y";
			VectorLabel0_2 = "Z";
			VectorLabel0_3 = "W";
#endif

			VectorComponentCount0 = 4;
			Color1= new MinMaxGradient(true);
			Vector1_0 = new MinMaxCurve(0.0f);
			Vector1_1 = new MinMaxCurve(0.0f);
			Vector1_2 = new MinMaxCurve(0.0f);
			Vector1_3 = new MinMaxCurve(0.0f);
#if UNIVERSAL
			ColorLabel1 = "Color";
			VectorLabel1_0 = "X";
			VectorLabel1_1 = "Y";
			VectorLabel1_2 = "Z";
			VectorLabel1_3 = "W";
#endif
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasLabel(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			Mode0 = (ParticleSystemCustomDataMode)reader.ReadInt32();
			VectorComponentCount0 = reader.ReadInt32();
			Color0.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				ColorLabel0 = reader.ReadString();
			}
#endif
			Vector0_0.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				VectorLabel0_0 = reader.ReadString();
			}
#endif
			Vector0_1.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				VectorLabel0_1 = reader.ReadString();
			}
#endif
			Vector0_2.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				VectorLabel0_2 = reader.ReadString();
			}
#endif
			Vector0_3.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				VectorLabel0_3 = reader.ReadString();
			}
#endif
			Mode1 = (ParticleSystemCustomDataMode)reader.ReadInt32();
			VectorComponentCount1 = reader.ReadInt32();
			Color1.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				ColorLabel1 = reader.ReadString();
			}
#endif
			Vector1_0.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				VectorLabel1_0 = reader.ReadString();
			}
#endif
			Vector1_1.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				VectorLabel1_1 = reader.ReadString();
			}
#endif
			Vector1_2.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				VectorLabel1_2 = reader.ReadString();
			}
#endif
			Vector1_3.Read(reader);
#if UNIVERSAL
			if (HasLabel(reader.Flags))
			{
				VectorLabel1_3 = reader.ReadString();
			}
#endif
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add(Mode0Name, (int)Mode0);
			node.Add(VectorComponentCount0Name, VectorComponentCount0);
			node.Add(Color0Name, Color0.ExportYAML(container));
			//node.Add("colorLabel0", HasLabel(container.Flags) ? ColorLabel0 : "Color");
			node.Add(Vector00Name, Vector0_0.ExportYAML(container));
			//node.Add("vectorLabel0_0", HasLabel(container.Flags) ? VectorLabel0_0 : "X");
			node.Add(Vector01Name, Vector0_1.ExportYAML(container));
			//node.Add("vectorLabel0_1", HasLabel(container.Flags) ? VectorLabel0_1 : "Y");
			node.Add(Vector02Name, Vector0_2.ExportYAML(container));
			//node.Add("vectorLabel0_2", HasLabel(container.Flags) ? VectorLabel0_2 : "Z");
			node.Add(Vector03Name, Vector0_3.ExportYAML(container));
			//node.Add("vectorLabel0_3", HasLabel(container.Flags) ? VectorLabel0_3 : "W");
			node.Add(Mode1Name, (int)Mode1);
			node.Add(VectorComponentCount1Name, VectorComponentCount1);
			node.Add(Color1Name, Color1.ExportYAML(container));
			//node.Add("colorLabel1", HasLabel(container.Flags) ? ColorLabel1 : "Color");
			node.Add(Vector10Name, Vector1_0.ExportYAML(container));
			//node.Add("vectorLabel1_0", HasLabel(container.Flags) ? VectorLabel1_0 : "X");
			node.Add(Vector11Name, Vector1_1.ExportYAML(container));
			//node.Add("vectorLabel1_1", HasLabel(container.Flags) ? VectorLabel1_1 : "Y");
			node.Add(Vector12Name, Vector1_2.ExportYAML(container));
			//node.Add("vectorLabel1_2", HasLabel(container.Flags) ? VectorLabel1_2 : "Z");
			node.Add(Vector13Name, Vector1_3.ExportYAML(container));
			//node.Add("vectorLabel1_3", HasLabel(container.Flags) ? VectorLabel1_3 : "W");
			return node;
		}

		public ParticleSystemCustomDataMode Mode0 { get; set; }
		public int VectorComponentCount0 { get; set; }
#if UNIVERSAL
		public string ColorLabel0 { get; set; }
		public string VectorLabel0_0 { get; private set; }
		public string VectorLabel0_1 { get; private set; }
		public string VectorLabel0_2 { get; private set; }
		public string VectorLabel0_3 { get; private set; }
#endif
		public ParticleSystemCustomDataMode Mode1 { get; set; }
		public int VectorComponentCount1 { get; set; }
#if UNIVERSAL
		public string ColorLabel1 { get; set; }
		public string VectorLabel1_0 { get; private set; }
		public string VectorLabel1_1 { get; private set; }
		public string VectorLabel1_2 { get; private set; }
		public string VectorLabel1_3 { get; private set; }
#endif

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
