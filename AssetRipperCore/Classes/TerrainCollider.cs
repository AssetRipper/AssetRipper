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
	public sealed class TerrainCollider : Collider
	{
		public TerrainCollider(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasTerrainMaterial(UnityVersion version) => version.IsGreaterEqual(5);

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(2, 6);

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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
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

		public PPtr<TerrainData.TerrainData> TerrainData = new();

		protected override bool IncludesMaterial => HasTerrainMaterial(SerializedFile.Version);
		protected override bool IncludesIsTrigger => false;
	}
}
