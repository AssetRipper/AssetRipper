using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.PhysicMaterials;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			DynamicFriction = stream.ReadSingle();
			StaticFriction = stream.ReadSingle();
			Bounciness = stream.ReadSingle();
			FrictionCombine = stream.ReadInt32();
			BounceCombine = stream.ReadInt32();

			if (IsReadFrictionDirection2(stream.Version))
			{
				FrictionDirection2.Read(stream);
				DynamicFriction2 = stream.ReadSingle();
				StaticFriction2 = stream.ReadSingle();
			}
			if (IsReadUseSpring(stream.Version))
			{
				UseSpring = stream.ReadBoolean();
				Spring.Read(stream);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
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
