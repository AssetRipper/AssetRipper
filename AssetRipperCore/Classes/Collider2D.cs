using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public abstract class Collider2D : Behaviour
	{
		protected Collider2D(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasDensity(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasUsedByEffector(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasUsedByComposite(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasOffset(UnityVersion version) => version.IsGreaterEqual(5);

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

		public PPtr<PhysicsMaterial2D> Material = new();
		public Vector2f Offset = new();
	}
}
