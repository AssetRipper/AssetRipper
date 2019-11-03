using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class TerrainCollider : Collider
	{
		public TerrainCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasTerrainMaterial(Version version) => version.IsGreaterEqual(5);

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlign(Version version) => version.IsGreaterEqual(2, 6);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			TerrainData.Read(reader);
			EnableTreeColliders = reader.ReadBoolean();
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(TerrainData, TerrainDataName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(TerrainDataName, TerrainData.ExportYAML(container));
			node.Add(EnableTreeCollidersName, EnableTreeColliders);
			return node;
		}

		/// <summary>
		/// CreateTreeColliders previously
		/// </summary>
		public bool EnableTreeColliders { get; set; }

		public const string TerrainDataName = "m_TerrainData";
		public const string EnableTreeCollidersName = "m_EnableTreeColliders";

		public PPtr<TerrainData> TerrainData;

		protected override bool IncludesMaterial => HasTerrainMaterial(File.Version);
		protected override bool IncludesIsTrigger => false;
	}
}
