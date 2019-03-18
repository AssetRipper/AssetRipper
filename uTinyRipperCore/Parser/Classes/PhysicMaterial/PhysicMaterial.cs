using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.PhysicMaterials;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class PhysicMaterial : NamedObject
	{
		public PhysicMaterial(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		public static bool IsReadFrictionDirection2(Version version)
		{
			return version.IsLess(5, 2);
		}
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public static bool IsReadUseSpring(Version version)
		{
			return version.IsLess(2);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DynamicFriction = reader.ReadSingle();
			StaticFriction = reader.ReadSingle();
			Bounciness = reader.ReadSingle();
			FrictionCombine = reader.ReadInt32();
			BounceCombine = reader.ReadInt32();

			if (IsReadFrictionDirection2(reader.Version))
			{
				FrictionDirection2.Read(reader);
				DynamicFriction2 = reader.ReadSingle();
				StaticFriction2 = reader.ReadSingle();
			}
			if (IsReadUseSpring(reader.Version))
			{
				UseSpring = reader.ReadBoolean();
				Spring.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("dynamicFriction", DynamicFriction);
			node.Add("staticFriction", StaticFriction);
			node.Add("bounciness", Bounciness);
			node.Add("frictionCombine", FrictionCombine);
			node.Add("bounceCombine", BounceCombine);
			return node;
		}

		public float DynamicFriction { get; private set; }
		public float StaticFriction { get; private set; }
		public float Bounciness { get; private set; }
		public int FrictionCombine { get; private set; }
		public int BounceCombine { get; private set; }
		public float DynamicFriction2 { get; private set; }
		public float StaticFriction2 { get; private set; }
		public bool UseSpring { get; private set; }

		public Vector3f FrictionDirection2;
		public JointSpring Spring;
	}
}
