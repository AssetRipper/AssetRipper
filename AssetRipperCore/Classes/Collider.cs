using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public abstract class Collider : Component
	{
		protected Collider(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		public static bool HasEnabled(UnityVersion version) => version.IsGreaterEqual(3, 4);

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsMaterialConditional(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		private static bool IsTriggerConditional(UnityVersion version) => version.IsGreaterEqual(5);

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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
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

		public PPtr<PhysicMaterial.PhysicMaterial> Material = new();

		protected abstract bool IncludesMaterial { get; }
		protected abstract bool IncludesIsTrigger { get; }
	}
}
