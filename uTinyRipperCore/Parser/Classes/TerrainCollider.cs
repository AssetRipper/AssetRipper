using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

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
		public static bool IsReadTerrainMaterial(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			TerrainData.Read(reader);
			EnableTreeColliders = reader.ReadBoolean();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return TerrainData.FetchDependency(file, isLog, ToLogString, "m_TerrainData");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_TerrainData", TerrainData.ExportYAML(container));
			node.Add("m_EnableTreeColliders", EnableTreeColliders);
			return node;
		}

		/// <summary>
		/// CreateTreeColliders previously
		/// </summary>
		public bool EnableTreeColliders { get; private set; }

		public PPtr<TerrainData> TerrainData;

		protected override bool IsReadMaterial => IsReadTerrainMaterial(File.Version);
		protected override bool IsReadIsTrigger => false;
	}
}
