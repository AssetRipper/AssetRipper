using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.NavMeshDatas;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class NavMeshSettings : LevelGameManager
	{
		public NavMeshSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadBuildSettings(TransferInstructionFlags flags)
		{
			return !flags.IsSerializeGameRelease();
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadBuildSettings(stream.Flags))
			{
				BuildSettings.Read(stream);
			}
			NavMeshData.Read(stream);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_BuildSettings", BuildSettings.ExportYAML(exporter));
			node.Add("m_NavMeshData", NavMeshData.ExportYAML(exporter));
			return node;
		}

		public NavMeshBuildSettings BuildSettings;
		public PPtr<NavMeshData> NavMeshData;
	}
}
