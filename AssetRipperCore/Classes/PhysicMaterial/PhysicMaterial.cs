using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.PhysicMaterial
{
	public sealed class PhysicMaterial : NamedObject
	{
		public PhysicMaterial(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		public static bool HasFrictionDirection2(UnityVersion version) => version.IsLess(5, 2);
		/// <summary>
		/// Less than 2.0.0
		/// </summary>
		public static bool HasUseSpring(UnityVersion version) => version.IsLess(2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DynamicFriction = reader.ReadSingle();
			StaticFriction = reader.ReadSingle();
			Bounciness = reader.ReadSingle();
			FrictionCombine = reader.ReadInt32();
			BounceCombine = reader.ReadInt32();

			if (HasFrictionDirection2(reader.Version))
			{
				FrictionDirection2.Read(reader);
				DynamicFriction2 = reader.ReadSingle();
				StaticFriction2 = reader.ReadSingle();
			}
			if (HasUseSpring(reader.Version))
			{
				UseSpring = reader.ReadBoolean();
				Spring.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(DynamicFrictionName, DynamicFriction);
			node.Add(StaticFrictionName, StaticFriction);
			node.Add(BouncinessName, Bounciness);
			node.Add(FrictionCombineName, FrictionCombine);
			node.Add(BounceCombineName, BounceCombine);
			return node;
		}

		public float DynamicFriction { get; set; }
		public float StaticFriction { get; set; }
		public float Bounciness { get; set; }
		public int FrictionCombine { get; set; }
		public int BounceCombine { get; set; }
		public float DynamicFriction2 { get; set; }
		public float StaticFriction2 { get; set; }
		public bool UseSpring { get; set; }

		public const string DynamicFrictionName = "dynamicFriction";
		public const string StaticFrictionName = "staticFriction";
		public const string BouncinessName = "bounciness";
		public const string FrictionCombineName = "frictionCombine";
		public const string BounceCombineName = "bounceCombine";

		public Vector3f FrictionDirection2 = new();
		public JointSpring Spring = new();
	}
}
