using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public abstract class Collider2D : Behaviour
	{
		protected Collider2D(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasDensity(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasUsedByEffector(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasUsedByComposite(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasOffset(Version version) => version.IsGreaterEqual(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasDensity(reader.Version))
			{
				Density = reader.ReadSingle();
			}
			Material.Read(reader);
			IsTrigger = reader.ReadBoolean();
			if (HasUsedByEffector(reader.Version))
			{
				UsedByEffector = reader.ReadBoolean();
			}
			if (HasUsedByComposite(reader.Version))
			{
				UsedByComposite = reader.ReadBoolean();
			}
			reader.AlignStream();

			if (HasOffset(reader.Version))
			{
				Offset.Read(reader);
			}
		}
		
		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
			
			yield return context.FetchDependency(Material, MaterialName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(DensityName, Density);
			node.Add(MaterialName, Material.ExportYAML(container));
			node.Add(IsTriggerName, IsTrigger);
			node.Add(UsedByEffectorName, UsedByEffector);
			node.Add(UsedByCompositeName, UsedByComposite);
			node.Add(OffsetName, Offset.ExportYAML(container));
			return node;
		}

		public float Density { get; set; }
		public bool IsTrigger { get; set; }
		public bool UsedByEffector { get; set; }
		public bool UsedByComposite { get; set; }

		public const string DensityName = "m_Density";
		public const string MaterialName = "m_Material";
		public const string IsTriggerName = "m_IsTrigger";
		public const string UsedByEffectorName = "m_UsedByEffector";
		public const string UsedByCompositeName = "m_UsedByComposite";
		public const string OffsetName = "m_Offset";

		public PPtr<PhysicsMaterial2D> Material;
		public Vector2f Offset;
	}
}
