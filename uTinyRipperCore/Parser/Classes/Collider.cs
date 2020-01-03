using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public abstract class Collider : Component
	{
		protected Collider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasEnabled(Version version) => version.IsGreaterEqual(3, 4);

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsMaterialConditional(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsTriggerConditional(Version version) => version.IsGreaterEqual(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (!IsMaterialConditional(reader.Version) || IncludesMaterial)
			{
				Material.Read(reader);
			}
			if (!IsTriggerConditional(reader.Version) || IncludesIsTrigger)
			{
				IsTrigger = reader.ReadBoolean();
			}
			if (HasEnabled(reader.Version))
			{
				Enabled = reader.ReadBoolean();
				reader.AlignStream();
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
			if (IncludesMaterial)
			{
				node.Add(MaterialName, Material.ExportYAML(container));
			}
			if (IncludesIsTrigger)
			{
				node.Add(IsTriggerName, IsTrigger);
			}
			node.Add(EnabledName, Enabled);
			return node;
		}

		protected void ReadComponent(AssetReader reader)
		{
			base.Read(reader);
		}
		
		public bool IsTrigger { get; set; }
		public bool Enabled { get; protected set; }

		public const string MaterialName = "m_Material";
		public const string IsTriggerName = "m_IsTrigger";
		public const string EnabledName = "m_Enabled";

		public PPtr<PhysicMaterial> Material;

		protected abstract bool IncludesMaterial { get; }
		protected abstract bool IncludesIsTrigger { get; }
	}
}
