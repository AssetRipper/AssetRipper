using uTinyRipper.AssetExporters;
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
		public static bool IsReadLabel(TransferInstructionFlags flags)
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
			if (IsReadLabel(reader.Flags))
			{
				ColorLabel0 = reader.ReadString();
			}
#endif
			Vector0_0.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				VectorLabel0_0 = reader.ReadString();
			}
#endif
			Vector0_1.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				VectorLabel0_1 = reader.ReadString();
			}
#endif
			Vector0_2.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				VectorLabel0_2 = reader.ReadString();
			}
#endif
			Vector0_3.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				VectorLabel0_3 = reader.ReadString();
			}
#endif
			Mode1 = (ParticleSystemCustomDataMode)reader.ReadInt32();
			VectorComponentCount1 = reader.ReadInt32();
			Color1.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				ColorLabel1 = reader.ReadString();
			}
#endif
			Vector1_0.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				VectorLabel1_0 = reader.ReadString();
			}
#endif
			Vector1_1.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				VectorLabel1_1 = reader.ReadString();
			}
#endif
			Vector1_2.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				VectorLabel1_2 = reader.ReadString();
			}
#endif
			Vector1_3.Read(reader);
#if UNIVERSAL
			if (IsReadLabel(reader.Flags))
			{
				VectorLabel1_3 = reader.ReadString();
			}
#endif
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = (YAMLMappingNode)base.ExportYAML(container);
			node.Add("mode0", (int)Mode0);
			node.Add("vectorComponentCount0", VectorComponentCount0);
			node.Add("color0", Color0.ExportYAML(container));
			//node.Add("colorLabel0", IsReadLabel(container.Flags) ? ColorLabel0 : "Color");
			node.Add("vector0_0", Vector0_0.ExportYAML(container));
			//node.Add("vectorLabel0_0", IsReadLabel(container.Flags) ? VectorLabel0_0 : "X");
			node.Add("vector0_1", Vector0_1.ExportYAML(container));
			//node.Add("vectorLabel0_1", IsReadLabel(container.Flags) ? VectorLabel0_1 : "Y");
			node.Add("vector0_2", Vector0_2.ExportYAML(container));
			//node.Add("vectorLabel0_2", IsReadLabel(container.Flags) ? VectorLabel0_2 : "Z");
			node.Add("vector0_3", Vector0_3.ExportYAML(container));
			//node.Add("vectorLabel0_3", IsReadLabel(container.Flags) ? VectorLabel0_3 : "W");
			node.Add("mode1", (int)Mode1);
			node.Add("vectorComponentCount1", VectorComponentCount1);
			node.Add("color1", Color1.ExportYAML(container));
			//node.Add("colorLabel1", IsReadLabel(container.Flags) ? ColorLabel1 : "Color");
			node.Add("vector1_0", Vector1_0.ExportYAML(container));
			//node.Add("vectorLabel1_0", IsReadLabel(container.Flags) ? VectorLabel1_0 : "X");
			node.Add("vector1_1", Vector1_1.ExportYAML(container));
			//node.Add("vectorLabel1_1", IsReadLabel(container.Flags) ? VectorLabel1_1 : "Y");
			node.Add("vector1_2", Vector1_2.ExportYAML(container));
			//node.Add("vectorLabel1_2", IsReadLabel(container.Flags) ? VectorLabel1_2 : "Z");
			node.Add("vector1_3", Vector1_3.ExportYAML(container));
			//node.Add("vectorLabel1_3", IsReadLabel(container.Flags) ? VectorLabel1_3 : "W");
			return node;
		}

		public ParticleSystemCustomDataMode Mode0 { get; private set; }
		public int VectorComponentCount0 { get; private set; }
#if UNIVERSAL
		public string ColorLabel0 { get; private set; }
		public string VectorLabel0_0 { get; private set; }
		public string VectorLabel0_1 { get; private set; }
		public string VectorLabel0_2 { get; private set; }
		public string VectorLabel0_3 { get; private set; }
#endif
		public ParticleSystemCustomDataMode Mode1 { get; private set; }
		public int VectorComponentCount1 { get; private set; }
#if UNIVERSAL
		public string ColorLabel1 { get; private set; }
		public string VectorLabel1_0 { get; private set; }
		public string VectorLabel1_1 { get; private set; }
		public string VectorLabel1_2 { get; private set; }
		public string VectorLabel1_3 { get; private set; }
#endif

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
