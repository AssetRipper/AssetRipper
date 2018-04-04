using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public class TerrainCollider : Collider
	{
		public TerrainCollider(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			if (IsAlign(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			TerrainData.Read(stream);
			EnableTreeColliders = stream.ReadBoolean();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			yield return TerrainData.FetchDependency(file, isLog, ToLogString, "m_TerrainData");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.Add("m_TerrainData", TerrainData.ExportYAML(exporter));
			node.Add("m_EnableTreeColliders", EnableTreeColliders);
			return node;
		}

		/// <summary>
		/// CreateTreeColliders previously
		/// </summary>
		public bool EnableTreeColliders { get; private set; }

		public PPtr<TerrainData> TerrainData;

		protected override bool IsReadMaterial => true;
		protected override bool IsReadIsTrigger => false;
	}
}
